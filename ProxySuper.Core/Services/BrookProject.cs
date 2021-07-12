using ProxySuper.Core.Models.Projects;
using Renci.SshNet;
using System;

namespace ProxySuper.Core.Services
{
    public class BrookProject : ProjectBase<BrookSettings>
    {
        private string brookServiceTemp = @"
        [Unit]
        Description=brook service
        After=network.target syslog.target
        Wants=network.target

        [Service]
        Type=simple
        ExecStart=##run_cmd##

        [Install]
        WantedBy=multi-user.target";

        public BrookProject(SshClient sshClient, BrookSettings parameters, Action<string> writeOutput) : base(sshClient, parameters, writeOutput)
        {
        }

        public override void Install()
        {

            WriteOutput("检测安装系统环境...");
            EnsureSystemEnv();
            WriteOutput("检测安装系统环境完成");

            WriteOutput("配置服务器端口...");
            ConfigFirewalld();
            WriteOutput("端口配置完成");

            WriteOutput("安装必要的系统工具...");
            ConfigureSoftware();
            WriteOutput("系统工具安装完成");

            if (Parameters.BrookType == BrookType.wssserver)
            {
                WriteOutput("检测域名是否绑定本机IP...");
                ValidateDomain();
                WriteOutput("域名检测完成");
            }

            InstallBrook();


            Console.WriteLine("*************安装完成，尽情享用吧**********");
        }

        public void InstallBrook()
        {
            Console.WriteLine("安装Brook");

            string url = "https://github.com/txthinking/brook/releases/latest/download/brook_linux_amd64";
            if (ArchType == ArchType.arm)
            {
                url = url.Replace("brook_linux_amd64", "brook_linux_arm7");
            }

            RunCmd($"curl -L {url} -o /usr/bin/brook");
            RunCmd("chmod +x /usr/bin/brook");
            Console.WriteLine("安装Brook完成");

            var brookService = brookServiceTemp.Replace("##run_cmd##", GetRunBrookCommand());

            RunCmd("rm -rf /etc/systemd/system/brook.service");
            RunCmd("touch /etc/systemd/system/brook.service");
            RunCmd($"echo \"{brookService}\" > /etc/systemd/system/brook.service");
            RunCmd("sudo chmod 777 /etc/systemd/system/brook.service");

            RunCmd("systemctl enable brook");
            RunCmd("systemctl restart brook");

            WriteOutput("********************");
            WriteOutput("安装完成，尽情想用吧~ ");
            WriteOutput("*********************");
        }

        private string GetRunBrookCommand()
        {
            var runBrookCmd = string.Empty;

            if (Parameters.BrookType == BrookType.server)
            {
                return $"/usr/bin/brook server --listen :{Parameters.Port} --password {Parameters.Password}";
            }

            if (Parameters.BrookType == BrookType.wsserver)
            {
                return $"/usr/bin/brook wsserver --listen :{Parameters.Port} --password {Parameters.Password}";
            }

            if (Parameters.BrookType == BrookType.wssserver)
            {
                return $"/usr/bin/brook wssserver --domain {Parameters.Domain} --password {Parameters.Password}";
            }

            if (Parameters.BrookType == BrookType.socks5)
            {
                var ip = OnlyIpv6 ? IPv6 : IPv4;
                return $"/usr/bin/brook socks5 --socks5 {ip}:{Parameters.Port}";
            }

            return runBrookCmd;
        }

        public void Uninstall()
        {
            RunCmd("systemctl stop brook");
            RunCmd("systemctl disable brook");
            RunCmd("rm -rf /etc/systemd/system/brook.service");
            RunCmd("rm -rf /usr/bin/brook");

            Console.WriteLine("关闭端口");
            ResetFirewalld();

            WriteOutput("******卸载完成******");
        }
    }
}
