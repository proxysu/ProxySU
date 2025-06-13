using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Converters;
using ProxySuper.Core.Helpers;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.Templates;
using Renci.SshNet.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProxySuper.Core.Services
{
    public class HysteriaService : ServiceBase<HysteriaSettings>
    {
        public HysteriaService(Host host, HysteriaSettings settings) : base(host, settings)
        {
        }


        public void Install()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    Progress.Step = "安装Hysteria";
                    Progress.Percentage = 0;


                    Progress.Desc = "检测系统环境";
                    EnsureRootUser();
                    EnsureSystemEnv();
                    Progress.Percentage = 20;

                    Progress.Desc = "安装必要的系统工具";
                    InstallSystemTools();
                    Progress.Percentage = 40;

                    Progress.Desc = "配置防火墙";
                    ConfigFirewalld();
                    Progress.Percentage = 50;

                    Progress.Step = "检测网络环境";
                    EnsureNetwork();
                    Progress.Percentage = 60;


                    Progress.Desc = "检测域名是否绑定本机IP";
                    ValidateDomain();
                    Progress.Percentage = 80;

                    Progress.Step = "上传Hysteria配置文件";
                    string yamlConfig = BuildYamlConfig();
                    UploadConfigFile(yamlConfig);
                    Progress.Step = "安装Hysteria服务";
                    InstallHysteria();

                    Progress.Percentage = 100;
                    Progress.Step = "安装Hysteria成功";
                    Progress.Desc = "安装Hysteria成功";

                    if (Settings.AutoACME == false)
                    {
                        Progress.Step = "请手动上传SSL证书";
                        Progress.Desc = "请手动上传SSL证书";
                    }

                    if (Settings.EnableUpDisguisedWeb == true)
                    {
                        Progress.Step = "请手动上传伪装网站内容";
                        Progress.Desc = "请手动上传伪装网站内容";
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Uninstall()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Progress.Step = "卸载Hysteria";
                    Progress.Percentage = 0;

                    Progress.Desc = "停止Hysteria服务";
                    RunCmd("systemctl stop hysteria-server.service");
                    RunCmd("systemctl disable hysteria-server.service");
                    Progress.Percentage = 30;

                    Progress.Desc = "删除Hysteria相关文件";
                    RunCmd("bash <(curl -fsSL https://get.hy2.sh/) --remove");
                    //RunCmd("rm -rf /etc/systemd/system/Hysteria.service");
                    //RunCmd("rm -rf /usr/bin/Hysteria");
                    Progress.Percentage = 80;

                    Progress.Desc = "重置防火墙设置";
                    ResetFirewalld();

                    Progress.Percentage = 100;
                    Progress.Desc = "卸载完成";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        /*
        private string HysteriaServiceTemp = @"
        [Unit]
        Description=hysteria service
        After=network.target syslog.target
        Wants=network.target

        [Service]
        Type=simple
        ExecStart=##run_cmd##

        [Install]
        WantedBy=multi-user.target";
        */
        private void InstallHysteria()
        {
            Progress.Desc = "执行Hysteria安装文件,及设置Hysteria服务";
            RunCmd($"bash <(curl -fsSL https://get.hy2.sh/)");

            /*
            string url = "https://github.com/apernet/hysteria/releases/download/v1.3.4/hysteria-linux-386";
            string targetPath = "/usr/bin/hysteria/hysteria-linux-386";

            if (ArchType == ArchType.arm)
            {
                url = url.Replace("hysteria-linux-386", "hysteria-linux-arm");
                targetPath = targetPath.Replace("hysteria-linux-386", "hysteria-linux-arm");
            }

            RunCmd($"curl -L {url} -o {targetPath}");
            RunCmd($"chmod +x {targetPath}");

            Progress.Desc = "设置Hysteria服务";
            var cmd = targetPath + " -c /usr/bin/hysteria/config.json server";
            var hysteriaService = HysteriaServiceTemp.Replace("##run_cmd##", cmd);

            RunCmd("rm -rf /etc/systemd/system/hysteria.service");
            RunCmd("touch /etc/systemd/system/hysteria.service");

            RunCmd($"echo \"{hysteriaService}\" > /etc/systemd/system/hysteria.service");
            RunCmd("sudo chmod 777 /etc/systemd/system/hysteria.service");

              */


            Progress.Desc = "启动Hysteria服务";
            RunCmd("systemctl enable hysteria-server.service");
            RunCmd("systemctl restart hysteria-server.service");
        }

        private static string ConfigFile_base = HysteriaConfigTemplates.ServerHysteria2Config_base;
        private static string ConfigFile_acme = HysteriaConfigTemplates.ServerHysteria2Config_acme;
        private static string ConfigFile_tls = HysteriaConfigTemplates.ServerHysteria2Config_tls;
        private static string ConfigFile_masquerade_proxy = HysteriaConfigTemplates.ServerHysteria2Config_masquerade_proxy;
        private static string ConfigFile_masquerade_file = HysteriaConfigTemplates.ServerHysteria2Config_masquerade_file;
        private static string ConfigFile_bbr = HysteriaConfigTemplates.ServerHysteria2Config_bbr;
        private static string ConfigFile_obfs = HysteriaConfigTemplates.ServerAndClientHysteria2Config_obfs;
        private static string ConfigFile_bandwidth = HysteriaConfigTemplates.ServerAndClientHysteria2Config_bandwidth;

        private static string ClientConfigFile_base = HysteriaConfigTemplates.ClientHysteria2Config_base;
        
     
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


        #region 私有方法

        private string BuildYamlConfig()
        {
            dynamic yamlConfigBase = LoadYamlConfigToJsonObject(ConfigFile_base);
            yamlConfigBase.listen = ":" + Settings.Port;
            yamlConfigBase.auth.password = Settings.Password;
            string yamlConfig = JsonObjectToYamlConfig(yamlConfigBase);

            dynamic yamlClientConfigBase = LoadYamlConfigToJsonObject(ClientConfigFile_base);
            yamlClientConfigBase.server = $"{Settings.Domain}:{Settings.Port}";
            yamlClientConfigBase.auth = Settings.Password;
            string yamlClientConfig = JsonObjectToYamlConfig(yamlClientConfigBase);

            if (Settings.AutoACME == true)
            {
                dynamic yamlConfigAcme = LoadYamlConfigToJsonObject(ConfigFile_acme);
                yamlConfigAcme.acme.domains[0] = Settings.Domain;
                yamlConfigAcme.acme.email = Settings.Email;
                string _yamlConfigAcme = JsonObjectToYamlConfig(yamlConfigAcme);
                yamlConfig = YamlConfigMerger.MergeYamlStrings(yamlConfig, _yamlConfigAcme, CamelCaseNamingConvention.Instance);
            }
            else
            {
                yamlConfig = YamlConfigMerger.MergeYamlStrings(yamlConfig, ConfigFile_tls, CamelCaseNamingConvention.Instance);
            }

            if (Settings.EnableObfs == true)
            {
                dynamic yamlConfigObfs = LoadYamlConfigToJsonObject(ConfigFile_obfs);
                yamlConfigObfs.obfs.salamander.password = Settings.ObfsPassword;
                string _yamlConfigObfs = JsonObjectToYamlConfig(yamlConfigObfs);
                yamlConfig = YamlConfigMerger.MergeYamlStrings(yamlConfig, _yamlConfigObfs, CamelCaseNamingConvention.Instance);

                dynamic yamlClientConfigObfs = LoadYamlConfigToJsonObject(ConfigFile_obfs);
                yamlClientConfigObfs.obfs.salamander.password = Settings.ObfsPassword;
                string _yamlClientConfigObfs = JsonObjectToYamlConfig(yamlClientConfigObfs);
                yamlClientConfig = YamlConfigMerger.MergeYamlStrings(yamlClientConfig, _yamlClientConfigObfs, CamelCaseNamingConvention.Instance);

            }

            if (Settings.EnableUpDisguisedWeb == true)
            {
                yamlConfig = YamlConfigMerger.MergeYamlStrings(yamlConfig, ConfigFile_masquerade_file, CamelCaseNamingConvention.Instance);
            }
            else if (string.IsNullOrEmpty(Settings.MaskDomain) == false)
            {
                dynamic yamlConfigMasquerade_proxy = LoadYamlConfigToJsonObject(ConfigFile_masquerade_proxy);

                var prefix = "https://";
                if (Settings.MaskDomain.StartsWith("http://"))
                {
                    prefix = "http://";
                }
                var domain = Settings.MaskDomain
                    .TrimStart("http://".ToCharArray())
                    .TrimStart("https://".ToCharArray());

                yamlConfigMasquerade_proxy.masquerade.proxy.url = $"{prefix}{domain}";// Settings.MaskDomain;
                string _yamlConfigMasquerade_proxy = JsonObjectToYamlConfig(yamlConfigMasquerade_proxy);
                yamlConfig = YamlConfigMerger.MergeYamlStrings(yamlConfig, _yamlConfigMasquerade_proxy, CamelCaseNamingConvention.Instance);
            }

            Settings.ClientHysteria2Config = yamlClientConfig;
            return yamlConfig;
        }
        
        private void UploadConfigFile(string yamlConfig)
        {
            RunCmd("mkdir -p /etc/hysteria");
            WriteToFile(yamlConfig, "/etc/hysteria/config.yaml");
        }

        private dynamic LoadYamlConfigToJsonObject(string yamlConfigString)
        {
            var JsonStr = JsonYamlConverters.YamlToJson(yamlConfigString);

            return JToken.Parse(JsonStr);
        }

        private string JsonObjectToYamlConfig(dynamic jsonObj)
        {
            var jsonStr = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            return JsonYamlConverters.JsonToYaml(jsonStr);
        }

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
                        RunCmd($"mv /etc/hysteria/ssl /etc/hysteria/{oldFileName}");

                        RunCmd("mkdir -p  /etc/hysteria/ssl");
                        UploadFile(stream, "/etc/hysteria/ssl/ssl.zip");
                        RunCmd("unzip  /etc/hysteria/ssl/ssl.zip -d  /etc/hysteria/ssl");
                    }

                    var crtFiles = RunCmd("find /etc/hysteria/ssl/*.crt").Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var keyFiles = RunCmd("find /etc/hysteria/ssl/*.key").Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (crtFiles.Length > 0 && keyFiles.Length > 0)
                    {
                        RunCmd($"mv {crtFiles[0]} /etc/hysteria/ssl/hysteria_ssl.crt");
                        RunCmd($"mv {keyFiles[0]} /etc/hysteria/ssl/hysteria_ssl.key");
                    }
                    else
                    {
                        Progress.Step = "上传失败";
                        Progress.Desc = "上传证书失败，缺少 .crt 和 .key 文件";
                        return;
                    }

                    Progress.Desc = "重启Hysteria服务";
                    RunCmd("systemctl restart hysteria");

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
                    if (!FileExists("/var/www/hymasq"))
                    {
                        RunCmd("mkdir -p /var/www/hymasq");
                    }
                    RunCmd("rm -rf /var/www/hymasq/*");
                    Progress.Percentage = 40;

                    Progress.Desc = "正在上传文件";
                    var file = sender as OpenFileDialog;
                    using (var stream = file.OpenFile())
                    {
                        UploadFile(stream, "/var/www/hymasq/webfile.zip");
                        RunCmd("unzip /var/www/hymasq/webfile.zip -d /var/www/hymasq");
                    }
                    RunCmd("chmod -R 777 /var/www/hymasq");
                    Progress.Percentage = 80;


                    Progress.Percentage = 90;

                    Progress.Desc = "重启Hysteria服务";
                    RunCmd("systemctl restart hysteria");
                    Progress.Percentage = 100;

                    Progress.Desc = "上传静态网站成功";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        #endregion
    }
}
