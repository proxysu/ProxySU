using Microsoft.Win32;
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
    public class TrojanGoService : ServiceBase<TrojanGoSettings>
    {
        public TrojanGoService(Host host, TrojanGoSettings settings) : base(host, settings)
        {
        }

        public void Install()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Progress.Percentage = 0;
                    Progress.Step = "安装TrojanGo";
                    Progress.Desc = "安装TrojanGo";
                    EnsureRootUser();

                    if (FileExists("/usr/local/bin/trojan-go"))
                    {
                        var btnResult = MessageBox.Show("已经安装Trojan-Go，是否需要重装？", "提示", MessageBoxButton.YesNo);
                        if (btnResult == MessageBoxResult.No)
                        {
                            MessageBox.Show("安装终止", "提示");
                            return;
                        }
                    }

                    var index = 1;
                    Progress.Step = $"{index++}. 检测系统环境";
                    EnsureSystemEnv();
                    Progress.Percentage = 10;

                    Progress.Step = $"{index++}. 安装必要的系统工具";
                    InstallSystemTools();
                    Progress.Percentage = 30;

                    Progress.Step = $"{index++}. 配置防火墙";
                    ConfigFirewalld();
                    Progress.Percentage = 40;

                    Progress.Step = $"{index++}. 检测网络环境";
                    EnsureNetwork();
                    Progress.Percentage = 50;

                    Progress.Step = $"{index++}. 检测域名是否解析到本机";
                    ValidateDomain();
                    Progress.Percentage = 60;

                    Progress.Step = $"{index++}. 安装Caddy服务";
                    InstallCaddy();
                    Progress.Percentage = 70;

                    Progress.Step = $"{index++}. 安装TrojanGo";
                    InstallTrojanGo();
                    Progress.Percentage = 80;

                    Progress.Step = $"{index++}. 上传Caddy配置文件";
                    UploadCaddySettings();
                    Progress.Percentage = 90;

                    Progress.Step = $"{index++}. 启动BBR";
                    EnableBBR();

                    Progress.Step = $"{index++}. 重启caddy服务";
                    RunCmd("systemctl restart caddy");

                    Progress.Desc = "启用Trojan-Go开机启动";
                    RunCmd("systemctl enable trojan-go");
                    RunCmd("systemctl restart trojan-go");

                    AppendCommand("分享连接：");
                    AppendCommand(ShareLink.BuildTrojanGo(Settings));

                    Progress.Percentage = 100;
                    Progress.Step = "安装成功";
                    Progress.Desc = string.Empty;

                    if (!Settings.WithTLS)
                    {
                        Progress.Step = "安装成功，请上传您的 TLS 证书。";
                    }
                    else
                    {
                        NavigationService.Navigate<TrojanGoConfigViewModel, TrojanGoSettings>(Settings);
                    }
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
                    EnsureRootUser();

                    Progress.Step = "卸载Trojgan-Go";
                    Progress.Percentage = 0;

                    Progress.Desc = "检测系统环境";
                    EnsureSystemEnv();
                    Progress.Percentage = 20;

                    Progress.Desc = "停止Trojan—Go服务";
                    RunCmd("systemctl stop trojan-go");
                    Progress.Percentage = 40;


                    Progress.Desc = "卸载Caddy";
                    UninstallCaddy();
                    Progress.Percentage = 60;

                    Progress.Desc = "卸载Trojan-Go";
                    RunCmd("rm -rf /usr/local/bin/trojan-go");
                    RunCmd("rm -rf /usr/local/etc/trojan-go");
                    Progress.Percentage = 90;

                    Progress.Desc = "删除 acme.sh";
                    RunCmd("acme.sh --uninstall");
                    RunCmd("rm -r  ~/.acme.sh");

                    Progress.Percentage = 100;
                    Progress.Step = "卸载Trojan-Go成功";
                    Progress.Desc = string.Empty;
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
                    Progress.Step = "更新配置文件";
                    Progress.Percentage = 0;

                    Progress.Desc = "检测系统环境";
                    EnsureRootUser();
                    EnsureSystemEnv();
                    Progress.Percentage = 30;

                    Progress.Desc = "更新配置文件";
                    UploadTrojanGoSettings();
                    Progress.Percentage = 70;

                    Progress.Desc = "重启caddy服务";
                    RunCmd("systemctl restart caddy");
                    Progress.Percentage = 80;

                    Progress.Desc = "重启Trojan-Go服务器";
                    RunCmd("systemctl restart trojan-go");

                    Progress.Percentage = 100;
                    Progress.Desc = "更新配置成功";
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

        public void UploadCert()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "压缩文件|*.zip";
            fileDialog.FileOk += DoUploadCert;
            fileDialog.ShowDialog();
        }

        public void ApplyForCert()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {

                    Progress.Step = "续签证书";
                    Progress.Percentage = 0;

                    Progress.Desc = "检测系统环境";
                    EnsureRootUser();
                    EnsureSystemEnv();
                    Progress.Percentage = 20;

                    Progress.Desc = "安装TLS证书";
                    InstallCert(
                        dirPath: "/usr/local/etc/trojan-go",
                        certName: "trojan-go.crt",
                        keyName: "trojan-go.key");
                    Progress.Percentage = 90;

                    Progress.Desc = "重启Trojan-go服务";
                    RunCmd("systemctl restart trojan-go");

                    Progress.Percentage = 100;
                    Progress.Step = "续签证书成功";
                    Progress.Desc = "续签证书成功";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        #region 私有方法


        private void DoUploadCert(object sender, CancelEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    EnsureRootUser();

                    Progress.Percentage = 0;
                    Progress.Step = "上传自有证书";
                    Progress.Desc = "检测系统环境";

                    EnsureSystemEnv();
                    Progress.Percentage = 20;

                    Progress.Desc = "正在上传文件";
                    var file = sender as OpenFileDialog;
                    using (var stream = file.OpenFile())
                    {
                        var oldFileName = $"ssl_{DateTime.Now.Ticks}";
                        RunCmd($"mv /usr/local/etc/trojan-go/ssl /usr/local/etc/trojan-go/{oldFileName}");

                        RunCmd("mkdir /usr/local/etc/trojan-go/ssl");
                        UploadFile(stream, "/usr/local/etc/trojan-go/ssl/ssl.zip");
                        RunCmd("unzip /usr/local/etc/trojan-go/ssl/ssl.zip -d /usr/local/etc/trojan-go/ssl");
                    }

                    var crtFiles = RunCmd("find /usr/local/etc/trojan-go/ssl/*.crt").Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var keyFiles = RunCmd("find /usr/local/etc/trojan-go/ssl/*.key").Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (crtFiles.Length > 0 && keyFiles.Length > 0)
                    {
                        RunCmd($"mv {crtFiles[0]} /usr/local/etc/trojan-go/ssl/trojan-go.crt");
                        RunCmd($"mv {keyFiles[0]} /usr/local/etc/trojan-go/ssl/trojan-go.key");
                    }
                    else
                    {
                        Progress.Step = "上传失败";
                        Progress.Desc = "上传证书失败，缺少 .crt 和 .key 文件";
                        return;
                    }

                    Progress.Desc = "重启trojan-go服务";
                    RunCmd("systemctl restart trojan-go");

                    Progress.Percentage = 100;
                    Progress.Desc = "上传证书完成";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

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

        private void UploadCaddySettings(bool useCustomWeb = false)
        {
            var config = TrojanGoConfigBuilder.BuildCaddyConfig(Settings, useCustomWeb);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(config));
            if (FileExists("/etc/caddy/Caddyfile"))
            {
                RunCmd("mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.back");
            }
            UploadFile(stream, "/etc/caddy/Caddyfile");
        }

        private void InstallTrojanGo()
        {
            RunCmd(@"curl https://raw.githubusercontent.com/proxysu/shellscript/master/trojan-go.sh yes | bash");
            var success = FileExists("/usr/local/bin/trojan-go");
            if (success == false)
            {
                throw new Exception("trojan-go 安装失败，请联系开发者！");
            }

            Progress.Desc = "设置Trojan-Go权限";
            RunCmd($"sed -i 's/User=nobody/User=root/g' /etc/systemd/system/trojan-go.service");
            RunCmd($"sed -i 's/CapabilityBoundingSet=/#CapabilityBoundingSet=/g' /etc/systemd/system/trojan-go.service");
            RunCmd($"sed -i 's/AmbientCapabilities=/#AmbientCapabilities=/g' /etc/systemd/system/trojan-go.service");
            RunCmd($"systemctl daemon-reload");

            if (Settings.WithTLS)
            {
                Progress.Desc = "安装TLS证书";
                InstallCert(
                    dirPath: "/usr/local/etc/trojan-go/ssl",
                    certName: "trojan-go.crt",
                    keyName: "trojan-go.key");
            }

            Progress.Desc = "上传Trojan-Go配置文件";
            UploadTrojanGoSettings();
        }

        private void UploadTrojanGoSettings()
        {
            // 上传配置
            Progress.Desc = "生成配置文件";
            var settings = TrojanGoConfigBuilder.BuildTrojanGoConfig(Settings);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(settings));

            Progress.Desc = "正在上传配置文件";
            UploadFile(stream, "/usr/local/etc/trojan-go/config.json");

        }

        #endregion


    }
}
