using ProxySU_Core.Tools;
using ProxySU_Core.ViewModels;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySU_Core.Models.Developers
{
    public enum CmdType
    {
        None,
        Apt,
        Dnf,
        Yum
    }

    public abstract class Project<TParameters> : BaseViewModel where TParameters : IParameters
    {
        private SshClient _sshClient;

        protected Action<string> WriteOutput;

        protected CmdType CmdType { get; set; }

        protected bool IsSELinux { get; set; }

        protected bool OnlyIpv6 { get; set; }

        protected string IPv4 { get; set; }

        protected string IPv6 { get; set; }

        protected TParameters Parameters { get; set; }

        public Project(SshClient sshClient, TParameters parameters, Action<string> writeOutput)
        {
            _sshClient = sshClient;
            WriteOutput = writeOutput;
            Parameters = parameters;
        }

        protected string RunCmd(string cmdStr)
        {
            var cmd = _sshClient.CreateCommand(cmdStr);
            WriteOutput(cmdStr);

            var result = cmd.Execute();
            WriteOutput(result);
            return result;
        }

        /// <summary>
        /// 执行安装命令
        /// </summary>
        public abstract void Install();

        /// <summary>
        /// 配置系统基础环境
        /// </summary>
        protected void EnsureSystemEnv()
        {
            string cmd;

            // 确认安装命令
            if (CmdType == CmdType.None)
            {
                cmd = RunCmd("command -v apt-get");
                if (!string.IsNullOrEmpty(cmd))
                {
                    CmdType = CmdType.Apt;
                }
            }

            if (CmdType == CmdType.None)
            {
                cmd = RunCmd("command -v dnf");
                if (!string.IsNullOrEmpty(cmd))
                {
                    CmdType = CmdType.Dnf;
                }
            }

            if (CmdType == CmdType.None)
            {
                cmd = RunCmd("command -v yum");
                if (!string.IsNullOrEmpty(cmd))
                {
                    CmdType = CmdType.Yum;
                }
            }

            // systemctl
            cmd = RunCmd("command -v systemctl");
            var hasSystemCtl = !string.IsNullOrEmpty(cmd);

            // SELinux
            cmd = RunCmd("command -v getenforce");
            IsSELinux = !string.IsNullOrEmpty(cmd);

            if (CmdType == CmdType.None || !hasSystemCtl)
            {
                throw new Exception("系统缺乏必要的安装组件如:apt-get||dnf||yum||Syetemd，主机系统推荐使用：CentOS 7/8,Debian 8/9/10,Ubuntu 16.04及以上版本");
            }


            // 判断是否启用了SELinux,如果启用了，并且工作在Enforcing模式下，则改为Permissive模式
            if (IsSELinux)
            {
                cmd = RunCmd("getenforce");

                // 检测到系统启用SELinux，且工作在严格模式下，需改为宽松模式
                if (cmd.Contains("Enforcing"))
                {
                    RunCmd("setenforce  0");
                    RunCmd(@"sed -i 's/SELINUX=enforcing/SELINUX=permissive/' /etc/selinux/config");
                }
            }
        }

        /// <summary>
        /// 确保Root账户登陆
        /// </summary>
        protected void EnsureRootAuth()
        {
            // 禁止一些可能产生的干扰信息
            RunCmd(@"sed -i 's/echo/#echo/g' ~/.bashrc");
            RunCmd(@"sed -i 's/echo/#echo/g' ~/.profile");


            // 检测是否运行在Root权限下
            var cmd = RunCmd("id -u");
            if (!cmd.Equals("0\n"))
            {
                throw new Exception("请使用Root账户登陆主机");
            }
        }

        /// <summary>
        /// 配置IPV6环境
        /// </summary>
        protected void ConfigureIPv6()
        {
            if (IsOnlyIpv6())
            {
                SetNat64();
            }
        }

        /// <summary>
        /// 配置必要的软件
        /// </summary>
        protected void ConfigureSoftware()
        {
            string cmd = RunCmd("command -v sudo");
            if (string.IsNullOrEmpty(cmd))
            {
                RunCmd(GetInstallCmd("sudo"));
            }

            // 安装curl,wget,unzip
            cmd = RunCmd("command -v curl");
            if (string.IsNullOrEmpty(cmd))
            {
                RunCmd(GetInstallCmd("curl"));
            }

            cmd = RunCmd("command -v wget");
            if (string.IsNullOrEmpty(cmd))
            {
                RunCmd(GetInstallCmd("wget"));
            }

            cmd = RunCmd("command -v unzip");
            if (string.IsNullOrEmpty(cmd))
            {
                RunCmd(GetInstallCmd("unzip"));
            }

            // 安装dig
            cmd = RunCmd("command -v dig");
            if (string.IsNullOrEmpty(cmd))
            {
                if (CmdType == CmdType.Apt)
                {
                    RunCmd(GetUpdateCmd());
                    RunCmd(GetInstallCmd("dnsutils"));
                }
                else if (CmdType == CmdType.Dnf)
                {
                    RunCmd(GetUpdateCmd());
                    RunCmd(GetInstallCmd("bind-utils"));
                }
                else if (CmdType == CmdType.Yum)
                {
                    RunCmd(GetUpdateCmd());
                    RunCmd(GetInstallCmd("bind-utils"));
                }
            }


            // 处理极其少见的xz-utils未安装的情况
            if (CmdType == CmdType.Apt)
            {
                RunCmd(GetInstallCmd("xz-utils"));
            }
            else
            {
                RunCmd(GetInstallCmd("xz-devel"));
            }

            // 检测是否安装lsof
            cmd = RunCmd("command -v lsof");
            if (string.IsNullOrEmpty(cmd))
            {
                RunCmd(GetInstallCmd("lsof"));
            }
        }

        protected void ClosePort(params int[] portList)
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
                    RunCmd($"firewall-cmd --zone=public --remove-port={port}/tcp --permanent");
                    RunCmd($"firewall-cmd --zone=public --remove-port={port}/udp --permanent");
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

        protected void OpenPort(params int[] portList)
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
                    RunCmd($"firewall-cmd --zone=public --add-port={port}/tcp --permanent");
                    RunCmd($"firewall-cmd --zone=public --add-port={port}/udp --permanent");
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
                        RunCmd($"ufw allow {port}/tcp");
                        RunCmd($"ufw allow {port}/udp");
                    }
                    RunCmd("yes | ufw reload");
                }
            }
        }

        /// <summary>
        /// 配置防火墙
        /// </summary>
        protected void ConfigureFirewall()
        {
            var portList = new List<int>();
            portList.Add(80);
            portList.Add(Parameters.Port);

            if (Parameters.Types.Contains(XrayType.ShadowsocksAEAD))
            {
                portList.Add(ConfigBuilder.ShadowSocksPort);
            }

            if (Parameters.Types.Contains(XrayType.VMESS_KCP))
            {
                portList.Add(ConfigBuilder.VMESS_mKCP_Port);
            }

            OpenPort(portList.ToArray());
        }

        /// <summary>
        /// 配置同步时间差
        /// </summary>
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

        /// <summary>
        /// 验证域名是否绑定了主机
        /// </summary>
        protected void ValidateDomain()
        {
            if (OnlyIpv6)
            {
                string cmdFilter = @"| grep  -oE '(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))' | head -n 1";
                var cmd = $"dig @resolver1.opendns.com AAAA {Parameters.Domain} +short -6 {cmdFilter}";
                var result = RunCmd(cmd).TrimEnd('\r', '\n');

                if (result == IPv6) return;
            }

            else
            {
                string cmdFilter = @"| grep  -oE '[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+' | head -n 1";
                var cmd = $"dig @resolver1.opendns.com A {Parameters.Domain} +short -4 {cmdFilter}";
                var result = RunCmd(cmd).TrimEnd('\r', '\n');

                if (result == IPv4) return;

            }


            var btnResult = MessageBox.Show(
                $"{Parameters.Domain}未能正常解析到服务器的IP，如果您使用了CDN请忽略，是否继续安装?", "提示", MessageBoxButton.YesNo);

            if (btnResult == MessageBoxResult.No)
            {
                throw new Exception($"域名解析失败，安装停止!");
            }

        }

        /// <summary>
        /// 判断是否安装某个软件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected bool FileExists(string path)
        {
            var cmdStr = $"if [[ -f {path} ]];then echo '1';else echo '0'; fi";
            var cmd = RunCmd(cmdStr);
            return cmd.Trim() == "1";
        }

        /// <summary>
        /// 安装 Caddy
        /// </summary>
        protected void InstallCaddy()
        {
            RunCmd("rm -rf caddy_install.sh");
            RunCmd("curl -o caddy_install.sh https://raw.githubusercontent.com/proxysu/shellscript/master/Caddy-Naive/caddy-naive-install.sh");
            RunCmd("yes | bash caddy_install.sh");
            RunCmd("rm -rf caddy_install.sh");
        }

        protected void UninstallCaddy()
        {
            RunCmd("rm -rf caddy_install.sh");
            RunCmd("curl -o caddy_install.sh https://raw.githubusercontent.com/proxysu/shellscript/master/Caddy-Naive/caddy-naive-install.sh");
            RunCmd("yes | bash caddy_install.sh uninstall");
            RunCmd("rm -rf caddy_install.sh");
            RunCmd("rm -rf /usr/share/caddy");
        }


        #region 检测系统环境

        private bool IsOnlyIpv6()
        {
            string cmd;

            cmd = RunCmd(@"curl -s https://api.ip.sb/ip --ipv4 --max-time 8");
            IPv4 = cmd.TrimEnd('\r', '\n');

            if (!string.IsNullOrEmpty(IPv4))
            {
                OnlyIpv6 = false;
                return false;
            }

            cmd = RunCmd(@"curl -s https://api.ip.sb/ip --ipv6 --max-time 8");
            IPv6 = cmd.TrimEnd('\r', '\n');

            if (string.IsNullOrEmpty(IPv6))
            {
                throw new Exception("未检测可用的的IP地址");
            }

            OnlyIpv6 = true;
            return OnlyIpv6;
        }

        private bool SetPortFree(int port, bool force = true)
        {
            string result = RunCmd($"lsof -n -P -i :{port} | grep LISTEN");

            if (!string.IsNullOrEmpty(result))
            {
                if (force)
                {
                    var btnResult = MessageBox.Show($"{port}端口被占用，将强制停止占用{port}端口的程序?", "提示", MessageBoxButton.YesNo);
                    if (btnResult == MessageBoxResult.No)
                    {
                        throw new Exception($"{port}端口被占用，安装停止!");
                    }

                    string[] process = result.Split(' ');
                    RunCmd($"systemctl stop {process[0]}");
                    RunCmd($"systemctl disable {process[0]}");
                    RunCmd($"pkill {process[0]}");
                    return SetPortFree(port, force: false);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public void ConfigurePort(bool force = true)
        {
            if (Parameters.Port == 80 || Parameters.Port == 443)
            {
                SetPortFree(80);
                SetPortFree(443);
            }
            else
            {
                SetPortFree(80);
                SetPortFree(443);
                SetPortFree(Parameters.Port);
            }
        }

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

        #endregion


        protected void UploadFile(Stream stream, string path)
        {
            using (var sftp = new SftpClient(_sshClient.ConnectionInfo))
            {
                sftp.Connect();
                sftp.UploadFile(stream, path, true);
                sftp.Disconnect();
            }
        }

        /// <summary>
        /// 根据系统环境匹配更新命令
        /// </summary>
        /// <returns></returns>
        protected string GetUpdateCmd()
        {
            if (CmdType == CmdType.Apt)
            {
                return "apt-get update";
            }
            else if (CmdType == CmdType.Dnf)
            {
                return "dnf clean all;dnf makecache";
            }
            else if (CmdType == CmdType.Yum)
            {
                return "yum clean all;yum makecache";
            }

            throw new Exception("未识别的系统");
        }

        /// <summary>
        /// 根据系统匹配安装命令
        /// </summary>
        /// <param name="soft"></param>
        /// <returns></returns>
        protected string GetInstallCmd(string soft)
        {
            if (CmdType == CmdType.Apt)
            {
                return "echo y | apt-get install " + soft;
            }
            else if (CmdType == CmdType.Dnf)
            {
                return "echo y | dnf -y install " + soft;
            }
            else if (CmdType == CmdType.Yum)
            {
                return "echo y | yum -y install " + soft;
            }

            throw new Exception("未识别的系统");
        }

    }
}
