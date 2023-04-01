using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using Renci.SshNet.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
                    UploadConfigFile();
                    Progress.Step = "安装Hysteria服务";
                    InstallHysteria();

                    Progress.Percentage = 100;
                    Progress.Step = "安装Hysteria成功";
                    Progress.Desc = "安装Hysteria成功";
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
                    RunCmd("systemctl stop Hysteria");
                    RunCmd("systemctl disable Hysteria");
                    Progress.Percentage = 30;

                    Progress.Desc = "删除Hysteria相关文件";
                    RunCmd("rm -rf /etc/systemd/system/Hysteria.service");
                    RunCmd("rm -rf /usr/bin/Hysteria");
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

        private void InstallHysteria()
        {
            Progress.Desc = "执行Hysteria安装文件";
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


            Progress.Desc = "启动Hysteria服务";
            RunCmd("systemctl enable hysteria");
            RunCmd("systemctl restart hysteria");
        }

        private const string ConfigFilePath = @"Templates\Hysteria\config.json";
        private void UploadConfigFile()
        {
            var text = File.ReadAllText(ConfigFilePath, Encoding.UTF8);
            var json = JsonConvert.DeserializeObject(text);
            var obj = JToken.FromObject(json) as dynamic;


            obj["listen"] = $":{Settings.Port}";
            obj["acme"]["domains"][0] = Settings.Domain;
            obj["email"] = Settings.Email;
            obj["obfs"] = Settings.Obfs;

            var configJson = JsonConvert.SerializeObject(
              obj,
              Formatting.Indented,
              new JsonSerializerSettings()
              {
                  NullValueHandling = NullValueHandling.Ignore
              });

            RunCmd("mkdir /usr/bin/hysteria");
            WriteToFile(configJson, "/usr/bin/hysteria/config.json");
        }
    }
}
