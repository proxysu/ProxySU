using ProxySuper.Core.Models.Projects;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.Services
{
    public class BrookProject : ProjectBase<BrookSettings>
    {
        public BrookProject(SshClient sshClient, BrookSettings parameters, Action<string> writeOutput) : base(sshClient, parameters, writeOutput)
        {
        }

        public override void Install()
        {

            WriteOutput("检测安装系统环境...");
            EnsureSystemEnv();
            WriteOutput("检测安装系统环境完成");

            WriteOutput("配置服务器端口...");
            ConfigurePort();
            WriteOutput("端口配置完成");

            WriteOutput("安装必要的系统工具...");
            ConfigureSoftware();
            WriteOutput("系统工具安装完成");

            WriteOutput("检测IP6...");
            ConfigureIPv6();
            WriteOutput("检测IP6完成");

            WriteOutput("配置防火墙...");
            ConfigureFirewall();
            WriteOutput("防火墙配置完成");

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


            var runBrookCmd = string.Empty;

            if (Parameters.BrookType == BrookType.server)
            {
                runBrookCmd = $"nohup /usr/bin/brook server --listen :{Parameters.Port} --password {Parameters.Password} &";
            }

            if (Parameters.BrookType == BrookType.wsserver)
            {
                runBrookCmd = $"nohup /usr/bin/brook wsserver --listen :{Parameters.Port} --password {Parameters.Password} &";
            }

            if (Parameters.BrookType == BrookType.wsserver)
            {
                runBrookCmd = $"nohup /usr/bin/brook wssserver --domain {Parameters.Domain} --password {Parameters.Password} &";
            }

            if (Parameters.BrookType == BrookType.socks5)
            {
                runBrookCmd = $"nohup /usr/bin/brook socks5 --socks5 :{Parameters.Port} &";
            }
        }

        public void Uninstall()
        {
            RunCmd("killall brook");

            Console.WriteLine("关闭端口");
            ClosePort(Parameters.FreePorts.ToArray());

            Console.WriteLine("******卸载完成******");
        }
    }
}
