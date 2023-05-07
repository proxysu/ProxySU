using MvvmCross;
using MvvmCross.Navigation;
using ProxySuper.Core.Helpers;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace ProxySuper.Core.Services
{
    public enum ArchType
    {
        x86,
        arm
    }

    public enum CmdType
    {
        None,
        Yum,
        Apt,
        Dnf
    }

    public abstract class ServiceBase<TSettings> where TSettings : IProjectSettings
    {
        private Host _host;

        private SshClient _sshClient;

        private ProjectProgress _progress;

        public ServiceBase(Host host, TSettings settings)
        {
            _host = host;

            Settings = settings;

            var connection = CreateConnectionInfo();
            if (connection != null)
            {
                _sshClient = new SshClient(connection);
            }

            _progress = new ProjectProgress();

            ArchType = ArchType.x86;

            CmdType = CmdType.None;

            IPv4 = string.Empty;

            IPv6 = string.Empty;

            IsOnlyIPv6 = false;

            NavigationService = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
        }

        public string RunCmd(string command)
        {
            AppendCommand(command);

            string result;
            if (_sshClient.IsConnected)
            {
                result = _sshClient.CreateCommand(command).Execute();
            }
            else
            {
                result = "连接已断开";
            }

            AppendCommand(result);

            return result;
        }

        public ProjectProgress Progress => _progress;

        public TSettings Settings { get; set; }

        public ArchType ArchType { get; set; }

        public CmdType CmdType { get; set; }

        public string IPv4 { get; set; }

        public string IPv6 { get; set; }

        public bool IsOnlyIPv6 { get; set; }

        public IMvxNavigationService NavigationService { get; set; }


        #region 公用方法
        public void Connect()
        {
            Task.Run(() =>
            {
                if (_sshClient == null)
                {
                    MessageBox.Show("无法建立连接，连接参数有误！");
                    return;
                }

                if (_sshClient.IsConnected == false)
                {
                    Progress.Desc = ("正在与服务器建立连接");
                    try
                    {
                        _sshClient.Connect();
                        Progress.Desc = ("建立连接成功");
                    }
                    catch (Exception e)
                    {
                        Progress.Desc = ("连接失败，" + e.Message);
                    }
                }
            });
        }

        public void Disconnect()
        {
            Task.Run(() =>
            {
                _sshClient?.Disconnect();
            });
        }

        protected void WriteToFile(string text, string path)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                using (var sftp = new SftpClient(_sshClient.ConnectionInfo))
                {
                    try
                    {
                        sftp.Connect();
                        sftp.UploadFile(stream, path, true);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sftp.Disconnect();
                    }
                }
            }
        }

        protected bool FileExists(string path)
        {
            var cmdStr = $"if [[ -f {path} ]];then echo '1';else echo '0'; fi";
            var cmd = RunCmd(cmdStr);
            return cmd.Trim() == "1";
        }

        protected void SyncTimeDiff()
        {
            RunCmd("rm -f /etc/localtime");
            RunCmd("ln -s /usr/share/zoneinfo/UTC /etc/localtime");

            var result = RunCmd("date +%s");
            var vpsSeconds = Convert.ToInt64(result);
            var localSeconds = (int)(DateTime.Now.ToUniversalTime() - DateTime.Parse("1970-01-01")).TotalSeconds;

            if (Math.Abs(vpsSeconds - localSeconds) >= 90)
            {
                // 同步本地时间
                var netUtcTime = DateTimeUtils.GetUTCTime();
                DateTimeUtils.SetDate(netUtcTime.ToLocalTime());

                // 同步VPS时间
                var utcTS = DateTimeUtils.GetUTCTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                long timeStampVPS = Convert.ToInt64(utcTS.TotalSeconds);
                RunCmd($"date --set=\"$(date \"+%Y-%m-%d %H:%M:%S\" -d @{timeStampVPS.ToString()})\"");
            }
        }

        protected void ValidateDomain()
        {
            var domainIP = RunCmd($"ping \"{Settings.Domain}\" -c 1" + @" | sed '1{s/[^(]*(//;s/).*//;q}'")
                .Trim('\r', '\n');

            if (IsOnlyIPv6)
            {
                Progress.Desc = ($"本机IP({IPv6})");
                if (IPv6 != domainIP)
                {
                    //throw new Exception("域名解析地址与服务器IP不匹配！");
                }
            }
            else
            {
                Progress.Desc = ($"本机IP({IPv4})");
                Progress.Desc = ($"域名IP({domainIP})");
                if (IPv4 != domainIP)
                {
                    //throw new Exception("域名解析地址与服务器IP不匹配！");
                }
            }
        }

        protected void EnableBBR()
        {
            Progress.Desc = ("检查系统是否满足启动BBR条件");
            var osVersion = RunCmd("uname -r");
            var canInstallBBR = CheckKernelVersionBBR(osVersion.Split('-')[0]);

            var bbrInfo = RunCmd("sysctl net.ipv4.tcp_congestion_control | grep bbr");
            var installed = bbrInfo.Contains("bbr");
            if (canInstallBBR && !installed)
            {
                RunCmd(@"bash -c 'echo ""net.core.default_qdisc=fq"" >> /etc/sysctl.conf'");
                RunCmd(@"bash -c 'echo ""net.ipv4.tcp_congestion_control=bbr"" >> /etc/sysctl.conf'");
                RunCmd(@"sysctl -p");

                if (IsOnlyIPv6)
                {
                    RemoveNat64();
                }
                Progress.Desc = ("启动BBR成功");
            }

            if (!canInstallBBR)
            {
                Progress.Desc = ("系统不满足启用BBR条件，启动失败。");
            }

        }

        /// <summary>
        /// 安装证书
        /// </summary>
        /// <param name="certPath"></param>
        /// <param name="keyPath"></param>
        protected void InstallCert(string dirPath, string certName, string keyName)
        {
            string certPath = dirPath + "/" + certName;
            string keyPath = dirPath + "/" + keyName;

            Progress.Desc = ("安装Acme软件");
            #region 安装Acme
            // 安装依赖
            RunCmd(GetInstallCmd("socat"));

            // 解决搬瓦工CentOS缺少问题
            RunCmd(GetInstallCmd("automake autoconf libtool"));

            // 安装Acme
            var result = RunCmd($"curl  https://get.acme.sh yes | sh");
            if (!result.Contains("nstall success"))
            {
                throw new Exception("安装 Acme 失败，请联系开发者！");
            }

            RunCmd("alias acme.sh=~/.acme.sh/acme.sh");

            #endregion


            #region 申请证书
            Progress.Desc = ("正在申请证书");
            // 申请证书 
            var cmd = $"/root/.acme.sh/acme.sh --force --debug --issue  --standalone  -d {Settings.Domain} {(IsOnlyIPv6 ? "--listen-v6" : "")} --pre-hook \"systemctl stop caddy\"  --post-hook  \"systemctl start caddy\" --server letsencrypt";
            result = RunCmd(cmd);


            if (result.Contains("success"))
            {
                Progress.Desc = ("申请证书成功");
            }
            else
            {
                Progress.Desc = ("申请证书失败，如果申请次数过多请更换二级域名，或联系开发者！");
                throw new Exception("申请证书失败，如果申请次数过多请更换二级域名，或联系开发者！");
            }
            #endregion

            // 安装证书
            Progress.Desc = ("安装TLS证书");
            RunCmd($"mkdir -p {dirPath}");
            RunCmd($"/root/.acme.sh/acme.sh  --installcert  -d {Settings.Domain}  --certpath {certPath} --keypath {keyPath}  --capath {certPath}");

            result = RunCmd($@"if [ ! -f ""{keyPath}"" ]; then echo ""0""; else echo ""1""; fi | head -n 1");

            if (result.Contains("1"))
            {
                Progress.Desc = ("安装证书成功");
            }
            else
            {
                Progress.Desc = ("安装证书失败，请联系开发者！");
                throw new Exception("安装证书失败，请联系开发者！");
            }

            RunCmd($"chmod 755 {dirPath}");
        }

        protected void UploadFile(Stream stream, string path)
        {
            using (var sftp = new SftpClient(_sshClient.ConnectionInfo))
            {
                sftp.Connect();
                sftp.UploadFile(stream, path, true);
                sftp.Disconnect();
            }
        }

        public void EnsureRootUser()
        {
            // 禁止一些可能产生的干扰信息
            RunCmd(@"sed -i 's/echo/#echo/g' ~/.bashrc");
            RunCmd(@"sed -i 's/echo/#echo/g' ~/.profile");

            var result = RunCmd("id -u");
            if (!result.Equals("0\n"))
            {
                throw new Exception("请使用Root权限账户登录！");
            }
        }

        public void UninstallCaddy()
        {
            Progress.Desc = "关闭Caddy服务";
            RunCmd("systemctl stop caddy");
            RunCmd("systemctl disable caddy");

            Progress.Desc = "彻底删除Caddy文件";
            RunCmd("rm -rf /etc/systemd/system/caddy.service");
            RunCmd("rm -rf /usr/bin/caddy");
            RunCmd("rm -rf /usr/share/caddy");
            RunCmd("rm -rf /etc/caddy");
        }

        public void EnsureSystemEnv()
        {
            // cpu架构
            Progress.Desc = ("检测CPU架构");
            EnsureCPU();

            // 安装命令类型
            Progress.Desc = ("检测系统安装命令");
            EnsureCmdType();

            // systemctl
            Progress.Desc = ("检测Systemctl");
            EnsureSystemctl();

            // SELinux
            Progress.Desc = ("检测SELinux");
            ConfigSELinux();
        }

        public void InstallSystemTools()
        {
            Progress.Desc = ("更新安装包");
            RunUpdateCmd();

            Progress.Desc = ("安装sudo工具");
            InstallSoftware("sudo");

            Progress.Desc = ("安装curl工具");
            InstallSoftware("curl");

            Progress.Desc = ("安装wget工具");
            InstallSoftware("wget");

            Progress.Desc = ("安装ping工具");
            InstallSoftware("ping");

            Progress.Desc = ("安装unzip工具");
            InstallSoftware("unzip");

            Progress.Desc = ("安装cron工具");
            InstallSoftware("cron");

            Progress.Desc = ("安装lsof工具");
            InstallSoftware("lsof");

            Progress.Desc = ("安装systemd工具");
            InstallSoftware("systemd");
        }

        public void ConfigFirewalld()
        {
            Progress.Desc = ("释放被占用的端口");
            Settings.FreePorts.ForEach(port => SetPortFree(port));

            Progress.Desc = ("开放需要的端口");
            OpenPort(Settings.FreePorts.ToArray());
        }

        public void ResetFirewalld()
        {
            ClosePort(Settings.FreePorts.ToArray());
        }

        public void EnsureNetwork()
        {
            string cmd;

            Progress.Desc = ("检测IPv4");
            cmd = RunCmd(@"curl -4 ip.sb");
            IPv4 = cmd.TrimEnd('\r', '\n');

            Progress.Desc = ($"IPv4地址为{IPv4}");
            if (!string.IsNullOrEmpty(IPv4))
            {
                IsOnlyIPv6 = false;
            }
            else
            {
                Progress.Desc = ("检测IPv6");
                cmd = RunCmd(@"curl -6 ip.sb");
                IPv6 = cmd.TrimEnd('\r', '\n');
                Progress.Desc = ($"IPv6地址为{IPv6}");

                IsOnlyIPv6 = true;
                SetNat64();
            }

            if (string.IsNullOrEmpty(IPv4) && string.IsNullOrEmpty(IPv6))
            {
                throw new Exception("未检测到服务器公网IP，请检查网络或重试。");
            }
        }

        public void InstallCaddy()
        {
            RunCmd("rm -rf caddy.tar.gz");
            RunCmd("rm -rf /etc/caddy");
            RunCmd("rm -rf /usr/share/caddy");

            var url = "https://github.com/caddyserver/caddy/releases/download/v2.4.3/caddy_2.4.3_linux_amd64.tar.gz";
            if (ArchType == ArchType.arm)
            {
                url = "https://github.com/caddyserver/caddy/releases/download/v2.4.3/caddy_2.4.3_linux_armv7.tar.gz";
            }

            RunCmd($"wget -O caddy.tar.gz {url}");
            RunCmd("mkdir /etc/caddy");
            RunCmd("tar -zxvf caddy.tar.gz -C /etc/caddy");
            RunCmd("cp -rf /etc/caddy/caddy /usr/bin");
            WriteToFile(Caddy.DefaultCaddyFile, "/etc/caddy/Caddyfile");
            WriteToFile(Caddy.Service, "/etc/systemd/system/caddy.service");
            RunCmd("systemctl daemon-reload");
            RunCmd("systemctl enable caddy");

            RunCmd("mkdir /usr/share/caddy");
            RunCmd("chmod 775 /usr/share/caddy");

            if (!FileExists("/usr/bin/caddy"))
            {
                throw new Exception("Caddy服务器安装失败，请联系开发者！");
            }
        }
        #endregion


        #region 检测System环境
        private void EnsureCPU()
        {
            var result = RunCmd("uname -m");
            if (result.Contains("x86"))
            {
                ArchType = ArchType.x86;
            }
            else if (result.Contains("arm") || result.Contains("arch"))
            {
                ArchType = ArchType.arm;
            }
        }

        private void EnsureCmdType()
        {
            var result = string.Empty;

            if (CmdType == CmdType.None)
            {
                result = RunCmd("command -v apt");
                if (!string.IsNullOrEmpty(result))
                {
                    CmdType = CmdType.Apt;
                }
            }

            if (CmdType == CmdType.None)
            {
                result = RunCmd("command -v dnf");
                if (!string.IsNullOrEmpty(result))
                {
                    CmdType = CmdType.Dnf;
                }
            }

            if (CmdType == CmdType.None)
            {
                result = RunCmd("command -v yum");
                if (!string.IsNullOrEmpty(result))
                {
                    CmdType = CmdType.Yum;
                }
            }

            if (CmdType == CmdType.None)
            {
                throw new Exception("未检测到正确的系统安装命令，请尝试使用ProxySU推荐的系统版本安装！");
            }
        }

        private void EnsureSystemctl()
        {
            var result = RunCmd("command -v systemctl");
            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("系统缺少 systemctl 组件，请尝试使用ProxySU推荐的系统版本安装！");
            }
        }

        private void ConfigSELinux()
        {
            // SELinux
            var result = RunCmd("command -v getenforce");
            var isSELinux = !string.IsNullOrEmpty(result);

            // 判断是否启用了SELinux,如果启用了，并且工作在Enforcing模式下，则改为Permissive模式
            if (isSELinux)
            {
                result = RunCmd("getenforce");

                // 检测到系统启用SELinux，且工作在严格模式下，需改为宽松模式
                if (result.Contains("Enforcing"))
                {
                    RunCmd("setenforce  0");
                    RunCmd(@"sed -i 's/SELINUX=enforcing/SELINUX=permissive/' /etc/selinux/config");
                }
            }
        }
        #endregion


        #region 私有方法

        protected void SetNat64()
        {
            var dns64List = FilterFastestIP();
            if (dns64List.Count == 0)
            {
                throw new Exception("未找到有效的Nat64网关");
            }

            var exists = FileExists("/etc/resolv.conf.proxysu");
            if (!exists)
            {
                var cmdStr = @"mv /etc/resolv.conf /etc/resolv.conf.proxysu";
                RunCmd(cmdStr);
            }

            foreach (var gateip in dns64List)
            {
                RunCmd($"echo \"nameserver   {gateip}\" > /etc/resolv.conf");
            }
        }

        protected void RemoveNat64()
        {
            RunCmd("rm /etc/resolv.conf");
            RunCmd("mv /etc/resolv.conf.proxysu /etc/resolv.conf");
        }

        protected void AppendCommand(string command)
        {
            if (!command.EndsWith("\n"))
            {
                command += "\n";
            }
            Progress.Logs += command;
        }

        private List<string> FilterFastestIP()
        {
            string[] gateNat64 = {
                "2a01:4f9:c010:3f02::1",
                "2001:67c:2b0::4",
                "2001:67c:2b0::6",
                "2a09:11c0:f1:bbf0::70",
                "2a01:4f8:c2c:123f::1",
                "2001:67c:27e4:15::6411",
                "2001:67c:27e4::64",
                "2001:67c:27e4:15::64",
                "2001:67c:27e4::60",
                "2a00:1098:2b::1",
                "2a03:7900:2:0:31:3:104:161",
                "2a00:1098:2c::1",
                "2a09:11c0:100::53",
            };

            Dictionary<string, float> dns64List = new Dictionary<string, float>();
            foreach (var gateip in gateNat64)
            {
                var cmdStr = $"ping6 -c4 {gateip} | grep avg | awk '{{print $4}}'|cut -d/ -f2";
                var cmd = RunCmd(cmdStr);
                if (!string.IsNullOrEmpty(cmd))
                {
                    if (float.TryParse(cmd, out float delay))
                    {
                        dns64List.Add(gateip, delay);
                    }
                }
            }

            return dns64List.Keys.ToList();
        }

        private bool CheckKernelVersionBBR(string kernelVer)
        {
            string[] linuxKernelCompared = kernelVer.Split('.');
            if (int.Parse(linuxKernelCompared[0]) > 4)
            {
                return true;
            }
            else if (int.Parse(linuxKernelCompared[0]) < 4)
            {
                return false;
            }
            else if (int.Parse(linuxKernelCompared[0]) == 4)
            {
                if (int.Parse(linuxKernelCompared[1]) >= 9)
                {
                    return true;
                }
                else if (int.Parse(linuxKernelCompared[1]) < 9)
                {
                    return false;
                }

            }
            return false;

        }

        private void SetPortFree(int port)
        {
            string result = RunCmd($"lsof -n -P -i :{port} | grep LISTEN");

            if (!string.IsNullOrEmpty(result))
            {
                string[] process = result.Split(' ');
                RunCmd($"systemctl stop {process[0]}");
                RunCmd($"systemctl disable {process[0]}");
                RunCmd($"pkill {process[0]}");
            }
        }

        private void OpenPort(params int[] portList)
        {
            string cmd;

            cmd = RunCmd("command -v firewall-cmd");
            if (!string.IsNullOrEmpty(cmd))
            {
                //有很奇怪的vps主机，在firewalld未运行时，端口是关闭的，无法访问。所以要先启动firewalld
                //用于保证acme.sh申请证书成功
                cmd = RunCmd("firewall-cmd --state");
                if (cmd.Trim() != "running")
                {
                    RunCmd("systemctl restart firewalld");
                }

                // 保持 ssh 端口开放
                RunCmd($"firewall-cmd --add-port={_host.Port}/tcp --permanent");
                foreach (var port in portList)
                {
                    RunCmd($"firewall-cmd --add-port={port}/tcp --permanent");
                    RunCmd($"firewall-cmd --add-port={port}/udp --permanent");
                }

                RunCmd("yes | firewall-cmd --reload");
            }
            else
            {
                cmd = RunCmd("command -v ufw");
                if (string.IsNullOrEmpty(cmd))
                {
                    RunCmd(GetInstallCmd("ufw"));
                    RunCmd("echo y | ufw enable");
                }

                // 保持 ssh 端口开放
                RunCmd($"ufw allow {_host.Port}/tcp");
                foreach (var port in portList)
                {
                    RunCmd($"ufw allow {port}/tcp");
                    RunCmd($"ufw allow {port}/udp");
                }
                RunCmd("yes | ufw reload");
            }
        }

        private void ClosePort(params int[] portList)
        {
            string cmd;

            cmd = RunCmd("command -v firewall-cmd");
            if (!string.IsNullOrEmpty(cmd))
            {
                //有很奇怪的vps主机，在firewalld未运行时，端口是关闭的，无法访问。所以要先启动firewalld
                //用于保证acme.sh申请证书成功
                cmd = RunCmd("firewall-cmd --state");
                if (cmd.Trim() != "running")
                {
                    RunCmd("systemctl restart firewalld");
                }

                foreach (var port in portList)
                {
                    RunCmd($"firewall-cmd --remove-port={port}/tcp --permanent");
                    RunCmd($"firewall-cmd --remove-port={port}/udp --permanent");
                }
                RunCmd("yes | firewall-cmd --reload");
            }
            else
            {
                cmd = RunCmd("command -v ufw");
                if (!string.IsNullOrEmpty(cmd))
                {
                    foreach (var port in portList)
                    {
                        RunCmd($"ufw delete allow {port}/tcp");
                        RunCmd($"ufw delete allow {port}/udp");
                    }
                    RunCmd("yes | ufw reload");
                }
            }
        }

        private void InstallSoftware(string software)
        {
            var result = RunCmd($"command -v {software}");
            if (string.IsNullOrEmpty(result))
            {
                RunCmd(GetInstallCmd(software));
            }
        }

        private string GetInstallCmd(string soft)
        {
            if (CmdType == CmdType.Apt)
            {
                return $"apt install -y {soft}";
            }
            else if (CmdType == CmdType.Yum)
            {
                return $"yum install -y {soft}";
            }
            else
            {
                return $"dnf install -y {soft}";
            }
        }

        private void RunUpdateCmd()
        {
            if (CmdType == CmdType.Apt)
            {
                RunCmd($"apt update -y");
            }
            else if (CmdType == CmdType.Yum)
            {
                RunCmd($"yum update -y");
            }
            else
            {
                RunCmd($"dnf update -y");
            }
        }


        private ConnectionInfo CreateConnectionInfo()
        {
            try
            {
                var authMethods = new List<AuthenticationMethod>();

                if (_host.SecretType == LoginSecretType.Password)
                {
                    authMethods.Add(new PasswordAuthenticationMethod(_host.UserName, _host.Password));
                }

                if (_host.SecretType == LoginSecretType.PrivateKey)
                {
                    PrivateKeyFile keyFile;
                    if (string.IsNullOrEmpty(_host.PrivateKeyPassPhrase))
                    {
                        keyFile = new PrivateKeyFile(_host.PrivateKeyPath);
                    }
                    else
                    {
                        keyFile = new PrivateKeyFile(_host.PrivateKeyPath, _host.PrivateKeyPassPhrase);
                    }
                    authMethods.Add(new PrivateKeyAuthenticationMethod(_host.UserName, keyFile));
                }

                if (_host.Proxy.Type == ProxyTypes.None)
                {
                    return new ConnectionInfo(
                        host: _host.Address,
                        username: _host.UserName,
                        port: _host.Port,
                        authenticationMethods: authMethods.ToArray());
                }

                return new ConnectionInfo(
                    host: _host.Address,
                    port: _host.Port,
                    username: _host.UserName,
                    proxyType: _host.Proxy.Type,
                    proxyHost: _host.Proxy.Address,
                    proxyPort: _host.Proxy.Port,
                    proxyUsername: _host.Proxy.UserName, proxyPassword: _host.Proxy.Password,
                    authenticationMethods: authMethods.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        #endregion
    }
}
