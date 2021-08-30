using Microsoft.Win32;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySuper.Core.Services
{
    public class V2rayService : ServiceBase<V2raySettings>
    {
        public V2rayService(Host host, V2raySettings settings) : base(host, settings)
        {
        }

        public void Install()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    int index = 1;
                    EnsureRootUser();

                    if (FileExists("/usr/local/bin/v2ray"))
                    {
                        var btnResult = MessageBox.Show("已经安装v2ray，是否需要重装？", "提示", MessageBoxButton.YesNo);
                        if (btnResult == MessageBoxResult.No)
                        {
                            MessageBox.Show("安装终止", "提示");
                            return;
                        }
                    }

                    Progress.Step = $"{index++}. 检测系统环境";
                    EnsureSystemEnv();
                    Progress.Percentage = 5;

                    Progress.Step = $"{index++}. 安装必要的系统工具";
                    InstallSystemTools();
                    Progress.Percentage = 15;

                    Progress.Step = $"{index++}. 配置防火墙";
                    ConfigFirewalld();
                    Progress.Percentage = 20;

                    Progress.Step = $"{index++}. 检测网络环境";
                    EnsureNetwork();
                    if (Settings.IsIPAddress)
                    {
                        Progress.Desc = ("检查域名是否解析正确");
                        ValidateDomain();
                    }
                    Progress.Percentage = 25;

                    Progress.Step = $"{index}. 同步系统和本地时间";
                    SyncTimeDiff();
                    Progress.Percentage = 30;

                    Progress.Step = $"{index++}. 安装Caddy服务器";
                    InstallCaddy();
                    Progress.Percentage = 50;

                    Progress.Step = $"{index++}. 安装V2ray-Core";
                    InstallV2ray();
                    Progress.Percentage = 80;

                    Progress.Step = $"{index++}. 上传Web服务器配置";
                    UploadCaddyFile();
                    Progress.Percentage = 90;

                    Progress.Step = $"{index++}. 启动BBR";
                    EnableBBR();

                    Progress.Desc = "重启V2ray服务";

                    RunCmd("systemctl restart caddy");
                    RunCmd("systemctl restart v2ray");

                    Progress.Percentage = 100;
                    Progress.Step = "安装成功";
                    Progress.Desc = string.Empty;

                    if (!Settings.WithTLS)
                    {
                        Progress.Step = "安装成功，请上传您的 TLS 证书。";
                    }
                    else
                    {
                        NavigationService.Navigate<V2rayConfigViewModel, V2raySettings>(Settings);
                    }
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
                    Progress.Step = "更新V2ray配置";
                    Progress.Percentage = 0;
                    EnsureRootUser();
                    var index = 0;

                    Progress.Desc = $"{index++}. 检测系统环境";
                    EnsureSystemEnv();
                    Progress.Percentage = 20;

                    Progress.Desc = $"{index++}. 配置防火墙";
                    RunCmd("systemctl stop v2ray");
                    RunCmd("systemctl stop caddy");
                    ConfigFirewalld();
                    Progress.Percentage = 40;

                    Progress.Desc = $"{index++}. 上传V2ray配置文件";
                    var configJson = V2rayConfigBuilder.BuildV2rayConfig(Settings);
                    WriteToFile(configJson, "/usr/local/etc/v2ray/config.json");
                    Progress.Percentage = 70;

                    Progress.Desc = $"{index++}. 上传Caddy配置文件";
                    UploadCaddyFile();
                    Progress.Percentage = 90;

                    Progress.Desc = $"{index++}. 重启v2ray服务";
                    RunCmd("systemctl restart caddy");
                    RunCmd("systemctl restart v2ray");
                    Progress.Percentage = 100;

                    Progress.Desc = ("更新配置成功");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        public void UpdateV2rayCore()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Progress.Step = "更新V2ray-Core";
                    Progress.Percentage = 0;

                    EnsureRootUser();
                    Progress.Percentage = 20;

                    Progress.Desc = "下载最新版本V2ray-Core";
                    EnsureSystemEnv();
                    Progress.Percentage = 40;

                    RunCmd("bash <(curl -L https://raw.githubusercontent.com/v2fly/fhs-install-v2ray/master/install-release.sh)");
                    RunCmd("systemctl restart v2ray");
                    Progress.Percentage = 100;

                    Progress.Desc = "更新V2ray-Core成功";
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

                    var index = 1;
                    Progress.Percentage = 0;

                    Progress.Step = $"{index++}. 检测系统环境";
                    Progress.Desc = "检测系统环境";
                    EnsureSystemEnv();
                    Progress.Percentage = 20;

                    Progress.Step = $"{index++}. 卸载Caddy服务";
                    UninstallCaddy();
                    Progress.Percentage = 40;

                    Progress.Step = $"{index++}. 卸载V2ray服务";
                    UninstallV2ray();
                    Progress.Percentage = 60;

                    Progress.Step = $"{index++}. 卸载Acme证书申请服务";
                    UninstallAcme();
                    Progress.Percentage = 80;

                    Progress.Step = $"{index++}. 重置防火墙端口";
                    ResetFirewalld();
                    Progress.Percentage = 100;

                    Progress.Step = "卸载完成";
                    Progress.Desc = "卸载完成";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        public void UploadCert()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "压缩文件|*.zip";
            fileDialog.FileOk += DoUploadCert;
            fileDialog.ShowDialog();
        }

        public void UploadWeb()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "压缩文件|*.zip";
            fileDialog.FileOk += DoUploadWeb;
            fileDialog.ShowDialog();
        }

        public void ApplyForCert()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Progress.Percentage = 0;
                    Progress.Step = "续签证书";

                    Progress.Desc = "检测系统环境";
                    EnsureRootUser();
                    EnsureSystemEnv();

                    Progress.Desc = "安装证书";
                    InstallCert(
                            dirPath: "/usr/local/etc/v2ray/ssl",
                            certName: "v2ray_ssl.crt",
                            keyName: "v2ray_ssl.key");

                    Progress.Percentage = 90;
                    Progress.Desc = "重启服务";
                    RunCmd("systemctl restart v2ray");

                    Progress.Percentage = 100;
                    Progress.Desc = "续签证书成功";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }


        #region 似有方法

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
                        RunCmd($"mv /usr/local/etc/v2ray/ssl /usr/local/etc/v2ray/{oldFileName}");

                        RunCmd("mkdir /usr/local/etc/v2ray/ssl");
                        UploadFile(stream, "/usr/local/etc/v2ray/ssl/ssl.zip");
                        RunCmd("unzip /usr/local/etc/v2ray/ssl/ssl.zip -d /usr/local/etc/v2ray/ssl");
                    }

                    var crtFiles = RunCmd("find /usr/local/etc/v2ray/ssl/*.crt").Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var keyFiles = RunCmd("find /usr/local/etc/v2ray/ssl/*.key").Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (crtFiles.Length > 0 && keyFiles.Length > 0)
                    {
                        RunCmd($"mv {crtFiles[0]} /usr/local/etc/v2ray/ssl/v2ray_ssl.crt");
                        RunCmd($"mv {keyFiles[0]} /usr/local/etc/v2ray/ssl/v2ray_ssl.key");
                    }
                    else
                    {
                        Progress.Step = "上传失败";
                        Progress.Desc = "上传证书失败，缺少 .crt 和 .key 文件";
                        return;
                    }

                    Progress.Desc = "重启V2ray服务";
                    RunCmd("systemctl restart v2ray");

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
                    Progress.Desc = "上传静态网站";
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
                    }
                    RunCmd("chmod -R 777 /usr/share/caddy");
                    Progress.Percentage = 80;

                    Progress.Desc = "上传Web配置文件";
                    UploadCaddyFile(useCustomWeb: true);
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

        private void InstallV2ray()
        {
            Progress.Desc = ("开始安装V2ray-Core");
            RunCmd("bash <(curl -L https://raw.githubusercontent.com/v2fly/fhs-install-v2ray/master/install-release.sh)");

            if (!FileExists("/usr/local/bin/v2ray"))
            {
                Progress.Desc = ("V2ray-Core安装失败，请联系开发者");
                throw new Exception("V2ray-Core安装失败，请联系开发者");
            }

            Progress.Desc = ("设置V2ray-Core权限");
            RunCmd($"sed -i 's/User=nobody/User=root/g' /etc/systemd/system/v2ray.service");
            RunCmd($"sed -i 's/CapabilityBoundingSet=/#CapabilityBoundingSet=/g' /etc/systemd/system/v2ray.service");
            RunCmd($"sed -i 's/AmbientCapabilities=/#AmbientCapabilities=/g' /etc/systemd/system/v2ray.service");
            RunCmd($"systemctl daemon-reload");
            RunCmd("systemctl enable v2ray");

            if (FileExists("/usr/local/etc/v2ray/config.json"))
            {
                RunCmd(@"mv /usr/local/etc/v2ray/config.json /usr/local/etc/v2ray/config.json.1");
            }
            Progress.Percentage = 60;

            if (Settings.WithTLS && !Settings.IsIPAddress)
            {
                Progress.Desc = ("安装TLS证书");
                InstallCert(
                    dirPath: "/usr/local/etc/v2ray/ssl",
                    certName: "v2ray_ssl.crt",
                    keyName: "v2ray_ssl.key");
                Progress.Percentage = 75;
            }

            Progress.Desc = ("生成v2ray服务器配置文件");
            var configJson = V2rayConfigBuilder.BuildV2rayConfig(Settings);
            WriteToFile(configJson, "/usr/local/etc/v2ray/config.json");
        }

        private void UploadCaddyFile(bool useCustomWeb = false)
        {
            var configJson = V2rayConfigBuilder.BuildCaddyConfig(Settings, useCustomWeb);

            if (FileExists("/etc/caddy/Caddyfile"))
            {
                RunCmd("mv /etc/caddy/Caddyfile /etc/caddy/Caddyfile.back");
            }
            WriteToFile(configJson, "/etc/caddy/Caddyfile");
        }


        private void UninstallV2ray()
        {
            Progress.Desc = "关闭V2ray服务";
            RunCmd("systemctl stop v2ray");
            RunCmd("systemctl disable v2ray");

            Progress.Desc = "卸载V2ray";
            RunCmd("bash <(curl -L https://raw.githubusercontent.com/v2fly/fhs-install-v2ray/master/install-release.sh) --remove");
        }

        private void UninstallAcme()
        {
            Progress.Desc = "卸载 acme.sh";
            RunCmd("acme.sh --uninstall");

            Progress.Desc = "删除 acme.sh 相关文件";
            RunCmd("rm -rf ~/.acme.sh");
        }

        #endregion
    }
}
