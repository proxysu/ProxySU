using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySuper.Core.Services
{
    public class NaiveProxyProject : ProjectBase<NaiveProxySettings>
    {
        public override void Install()
        {
            try
            {
                EnsureRootAuth();

                if (FileExists("/usr/local/bin/trojan-go"))
                {
                    var btnResult = MessageBox.Show("已经安装Trojan-Go，是否需要重装？", "提示", MessageBoxButton.YesNo);
                    if (btnResult == MessageBoxResult.No)
                    {
                        MessageBox.Show("安装终止", "提示");
                        return;
                    }
                }

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

                WriteOutput("同步系统和本地时间...");
                SyncTimeDiff();
                WriteOutput("时间同步完成");

                WriteOutput("检测域名是否绑定本机IP...");
                ValidateDomain();
                WriteOutput("域名检测完成");

                WriteOutput("安装Trojan-Go...");
                // InstallTrojanGo();
                WriteOutput("Trojan-Go安装完成");

                WriteOutput("安装Caddy...");
                InstallCaddy();
                // UploadCaddyFile();
                WriteOutput("Caddy安装完成");

                WriteOutput("启动BBR");
                EnableBBR();

                RunCmd("systemctl restart trojan-go");
                WriteOutput("************");
                WriteOutput("安装完成，尽情享用吧......");
                WriteOutput("************");
            }
            catch (Exception ex)
            {
                var errorLog = "安装终止，" + ex.Message;
                WriteOutput(errorLog);
                MessageBox.Show(errorLog);
            }
        }

        private void InstallNaiveProxy()
        {
            WriteOutput("安装 NaiveProxy");
            RunCmd(@"curl https://raw.githubusercontent.com/proxysu/shellscript/master/Caddy-Naive/caddy-naive-install.sh yes | bash");
            var success = FileExists("/usr/local/bin/trojan-go");
            if (success == false)
            {
                throw new Exception("trojan-go 安装失败，请联系开发者！");
            }

            RunCmd($"sed -i 's/User=nobody/User=root/g' /etc/systemd/system/trojan-go.service");
            RunCmd($"sed -i 's/CapabilityBoundingSet=/#CapabilityBoundingSet=/g' /etc/systemd/system/trojan-go.service");
            RunCmd($"sed -i 's/AmbientCapabilities=/#AmbientCapabilities=/g' /etc/systemd/system/trojan-go.service");
            RunCmd($"systemctl daemon-reload");

            RunCmd("systemctl enable trojan-go");
            RunCmd("systemctl start trojan-go");
            WriteOutput("NaiveProxy 安装完成");
        }
    }
}
