using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Models.Projects;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySuper.Core.Services
{
    public class NaiveProxyProject : ProjectBase<NaiveProxySettings>
    {
        public NaiveProxyProject(SshClient sshClient, NaiveProxySettings parameters, Action<string> writeOutput) : base(sshClient, parameters, writeOutput)
        {
        }

        public void Uninstall()
        {
            RunCmd("rm -rf caddy_install.sh");
            RunCmd("curl -o caddy_install.sh https://raw.githubusercontent.com/proxysu/shellscript/master/Caddy-Naive/caddy-naive-install.sh");
            RunCmd("yes | bash caddy_install.sh uninstall");
            RunCmd("rm -rf caddy_install.sh");
            WriteOutput("ProxyNaive卸载完成");
        }

        public void UploadWeb(Stream stream)
        {
            EnsureRootAuth();
            EnsureSystemEnv();

            if (!FileExists("/usr/share/caddy"))
            {
                RunCmd("mkdir /usr/share/caddy");
            }
            RunCmd("rm -rf /usr/share/caddy/*");
            UploadFile(stream, "/usr/share/caddy/caddy.zip");
            RunCmd("unzip /usr/share/caddy/caddy.zip -d /usr/share/caddy");
            RunCmd("chmod -R 777 /usr/share/caddy");
            UploadCaddyFile(useCustomWeb: true);
            WriteOutput("************ 上传网站模板完成 ************");
        }

        public override void Install()
        {
            try
            {
                EnsureRootAuth();

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

                WriteOutput("安装NaiveProxy...");
                InstallNaiveProxy();
                WriteOutput("NaiveProxy安装完成");

                WriteOutput("启动BBR");
                EnableBBR();

                WriteOutput("************");
                WriteOutput("安装完成，尽情享用吧......");
                WriteOutput("************");
            }
            catch (Exception ex)
            {
                var errorLog = "安装终止，" + ex.Message;
                WriteOutput(errorLog);
                MessageBox.Show("安装失败，请联系开发者或上传日志文件(Logs文件夹下)到github提问。");
            }
        }

        private void InstallNaiveProxy()
        {
            WriteOutput("安装 NaiveProxy");
            RunCmd(@"curl https://raw.githubusercontent.com/proxysu/shellscript/master/Caddy-Naive/caddy-naive-install.sh yes | bash");
            // 允许开机启动
            RunCmd("systemctl enable caddy");
            UploadCaddyFile(false);
            ConfigNetwork();
            WriteOutput("NaiveProxy 安装完成");
        }

        private void ConfigNetwork()
        {
            WriteOutput("优化网络参数");
            RunCmd(@"bash -c 'echo ""fs.file-max = 51200"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.core.rmem_max = 67108864"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.core.wmem_max = 67108864"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.core.rmem_default = 65536"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.core.wmem_default = 65536"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.core.netdev_max_backlog = 4096"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.core.somaxconn = 4096"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.ipv4.tcp_syncookies = 1"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.ipv4.tcp_tw_reuse = 1"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.ipv4.tcp_tw_recycle = 0"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.ipv4.tcp_fin_timeout = 30"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.ipv4.tcp_keepalive_time = 1200"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.ipv4.ip_local_port_range = 10000 65000"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.ipv4.tcp_max_syn_backlog = 4096"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.ipv4.tcp_max_tw_buckets = 5000"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.ipv4.tcp_rmem = 4096 87380 67108864"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.ipv4.tcp_wmem = 4096 65536 67108864"" >> /etc/sysctl.conf'");
            RunCmd(@"bash -c 'echo ""net.ipv4.tcp_mtu_probing = 1"" >> /etc/sysctl.conf'");
            RunCmd(@"sysctl -p");
            WriteOutput("网络参数优化完成");
        }

        private void UploadCaddyFile(bool useCustomWeb = false)
        {
            var caddyStr = BuildConfig(useCustomWeb);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(caddyStr));

            if (FileExists("/etc/caddy/Caddyfile"))
            {
                RunCmd("mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.back");
            }
            UploadFile(stream, "/etc/caddy/Caddyfile");
            RunCmd("systemctl restart caddy");
        }

        private string BuildConfig(bool useCustomWeb = false)
        {
            var jsonStr = File.ReadAllText("Templates/NaiveProxy/naive_server.caddyfile");
            jsonStr = jsonStr.Replace("##port##", Parameters.Port.ToString());
            jsonStr = jsonStr.Replace("##domain##", Parameters.Domain);
            jsonStr = jsonStr.Replace("##basicauth##", $"basic_auth {Parameters.UserName} {Parameters.Password}");

            if (!useCustomWeb && !string.IsNullOrEmpty(Parameters.MaskDomain))
            {
                var prefix = "http://";
                if (Parameters.MaskDomain.StartsWith("https://"))
                {
                    prefix = "https://";
                }
                var domain = Parameters.MaskDomain
                    .TrimStart("http://".ToCharArray())
                    .TrimStart("https://".ToCharArray());

                jsonStr = jsonStr.Replace("##reverse_proxy##", $"reverse_proxy {prefix}{domain} {{ \n        header_up Host {domain} \n    }}");
            }
            else
            {
                jsonStr = jsonStr.Replace("##reverse_proxy##", "");
                jsonStr = jsonStr.Replace("#file_server", "file_server");
            }
            return jsonStr;
        }
    }
}
