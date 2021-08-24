using Microsoft.Win32;
using MvvmCross.ViewModels;
using ProxySuper.Core.Models;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySuper.Core.Services
{
    public class NaiveProxyService : ServiceBase<NaiveProxySettings>
    {
        public NaiveProxyService(Host host, NaiveProxySettings settings) : base(host, settings)
        {
        }

        public void Install()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var index = 1;

                    Progress.Step = $"{index++}. 检测系统环境";
                    EnsureRootUser();
                    EnsureSystemEnv();
                    Progress.Percentage = 20;

                    Progress.Step = $"{index++}. 安装系统必要工具";
                    InstallSystemTools();
                    Progress.Percentage = 30;

                    Progress.Step = $"{index++}. 配置防火墙";
                    ConfigFirewalld();
                    Progress.Percentage = 40;

                    Progress.Step = $"{index++}. 检测网络环境";
                    EnsureNetwork();
                    Progress.Percentage = 50;

                    Progress.Step = $"{index++}. 检测域名是否绑定到本机";
                    ValidateDomain();
                    Progress.Percentage = 60;

                    Progress.Step = $"{index++}. 安装NaiveProxy";
                    InstallNaiveProxy();
                    Progress.Percentage = 80;

                    Progress.Step = $"{index++}. 优化网络参数";
                    ConfigNetwork();
                    Progress.Percentage = 90;

                    Progress.Step = $"{index++}. 启动BBR";
                    EnableBBR();

                    Progress.Desc = "重启Caddy服务";
                    RunCmd("systemctl restart caddy");

                    Progress.Percentage = 100;
                    Progress.Step = "NaiveProxy安装成功";
                    Progress.Desc = string.Empty;

                    AppendCommand("分享连接：");
                    AppendCommand(ShareLink.BuildNaiveProxy(Settings));

                    NavigationService.Navigate<NaiveProxyConfigViewModel, NaiveProxySettings>(Settings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        public void Uninstall()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Progress.Step = "卸载NaiveProxy";
                    Progress.Percentage = 0;

                    Progress.Desc = "正在卸载...";
                    RunCmd("rm -rf caddy_install.sh");
                    Progress.Percentage = 10;

                    RunCmd("curl -o caddy_install.sh https://raw.githubusercontent.com/proxysu/shellscript/master/Caddy-Naive/caddy-naive-install.sh");
                    Progress.Percentage = 20;

                    RunCmd("yes | bash caddy_install.sh uninstall");
                    Progress.Percentage = 80;

                    RunCmd("rm -rf caddy_install.sh");
                    Progress.Percentage = 100;
                    Progress.Step = "卸载NaiveProxy成功";
                    Progress.Desc = "卸载NaiveProxy成功";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        public void UpdateSettings()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Progress.Step = "更新配置";
                    Progress.Percentage = 0;

                    Progress.Desc = "检测系统环境";
                    EnsureRootUser();
                    EnsureSystemEnv();
                    Progress.Percentage = 30;

                    UploadCaddySettings();
                    Progress.Desc = "重启Caddy服务";
                    RunCmd("systemctl restart caddy");
                    Progress.Percentage = 100;

                    Progress.Step = "更新配置成功";
                    Progress.Desc = string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        public void UploadWeb()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "压缩文件|*.zip";
            fileDialog.FileOk += DoUploadWeb;
            fileDialog.ShowDialog();
        }


        #region 私有方法

        private void DoUploadWeb(object sender, CancelEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    EnsureRootUser();

                    Progress.Step = "上传静态网站";
                    Progress.Percentage = 0;

                    Progress.Desc = "检测系统环境";
                    EnsureSystemEnv();
                    Progress.Percentage = 20;

                    Progress.Desc = "创建网站目录";
                    if (!FileExists("/usr/share/caddy"))
                    {
                        RunCmd("mkdir /usr/share/caddy");
                    }
                    RunCmd("rm -rf /usr/share/caddy/*");
                    Progress.Percentage = 40;

                    Progress.Desc = "正在上传文件";
                    var file = sender as OpenFileDialog;
                    using (var stream = file.OpenFile())
                    {
                        UploadFile(stream, "/usr/share/caddy/caddy.zip");
                        RunCmd("unzip /usr/share/caddy/caddy.zip -d /usr/share/caddy");
                        RunCmd("chmod -R 777 /usr/share/caddy");
                        Progress.Percentage = 700;
                    }

                    Progress.Desc = "上传Caddy配置文件";
                    UploadCaddySettings(useCustomWeb: true);
                    Progress.Percentage = 90;

                    Progress.Desc = "重启caddy服务";
                    RunCmd("systemctl restart caddy");
                    Progress.Percentage = 100;
                    Progress.Desc = "上传静态网站成功";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void InstallNaiveProxy()
        {
            Progress.Desc = "下载NaiveProxy安装文件";
            RunCmd(@"curl https://raw.githubusercontent.com/proxysu/shellscript/master/Caddy-Naive/caddy-naive-install.sh yes | bash");

            Progress.Desc = "设置NaiveProxy开机启动";
            RunCmd("systemctl enable caddy");

            Progress.Desc = "上传配置文件";
            UploadCaddySettings(false);
        }

        private void ConfigNetwork()
        {
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
        }

        private void UploadCaddySettings(bool useCustomWeb = false)
        {
            Progress.Desc = "生成配置文件";
            var caddyStr = BuildConfig(useCustomWeb);

            if (FileExists("/etc/caddy/Caddyfile"))
            {
                RunCmd("mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.back");
            }

            Progress.Desc = "上传配置文件";
            WriteToFile(caddyStr, "/etc/caddy/Caddyfile");
        }

        private string BuildConfig(bool useCustomWeb = false)
        {
            var jsonStr = File.ReadAllText("Templates/NaiveProxy/naive_server.caddyfile");
            jsonStr = jsonStr.Replace("##port##", Settings.Port.ToString());
            jsonStr = jsonStr.Replace("##domain##", Settings.Domain);
            jsonStr = jsonStr.Replace("##basicauth##", $"basic_auth {Settings.UserName} {Settings.Password}");

            if (!useCustomWeb && !string.IsNullOrEmpty(Settings.MaskDomain))
            {
                var prefix = "http://";
                if (Settings.MaskDomain.StartsWith("https://"))
                {
                    prefix = "https://";
                }
                var domain = Settings.MaskDomain
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

        #endregion
    }
}
