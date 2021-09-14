using ProxySuper.Core.Models.Hosts;
using ProxySuper.Core.Models.Projects;
using ProxySuper.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProxySuper.Core.Services
{
    public class BrookService : ServiceBase<BrookSettings>
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


        public BrookService(Host host, BrookSettings settings) : base(host, settings)
        {
        }

        public void Install()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Progress.Step = "安装Brook";
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
                    if (Settings.BrookType == BrookType.wssserver)
                    {
                        Progress.Desc = "检测域名是否绑定本机IP";
                        ValidateDomain();
                        Progress.Percentage = 80;
                    }

                    Progress.Step = "安装Brook服务";
                    InstallBrook();

                    Progress.Percentage = 100;
                    Progress.Step = "安装Brook成功";
                    Progress.Desc = "安装Brook成功";

                    AppendCommand("分享连接：");
                    AppendCommand(ShareLink.BuildBrook(Settings));

                    NavigationService.Navigate<BrookConfigViewModel, BrookSettings>(Settings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        public void InstallBrook()
        {
            Progress.Desc = "执行Brook安装文件";
            string url = "https://github.com/txthinking/brook/releases/latest/download/brook_linux_amd64";
            if (ArchType == ArchType.arm)
            {
                url = url.Replace("brook_linux_amd64", "brook_linux_arm7");
            }

            RunCmd($"curl -L {url} -o /usr/bin/brook");
            RunCmd("chmod +x /usr/bin/brook");

            Progress.Desc = "设置Brook服务";
            var brookService = brookServiceTemp.Replace("##run_cmd##", GetRunBrookCommand());

            RunCmd("rm -rf /etc/systemd/system/brook.service");
            RunCmd("touch /etc/systemd/system/brook.service");
            RunCmd($"echo \"{brookService}\" > /etc/systemd/system/brook.service");
            RunCmd("sudo chmod 777 /etc/systemd/system/brook.service");

            Progress.Desc = "启动Brook服务";
            RunCmd("systemctl enable brook");
            RunCmd("systemctl restart brook");
        }

        private string GetRunBrookCommand()
        {
            var runBrookCmd = string.Empty;

            if (Settings.BrookType == BrookType.server)
            {
                return $"/usr/bin/brook server --listen :{Settings.Port} --password {Settings.Password}";
            }

            if (Settings.BrookType == BrookType.wsserver)
            {
                return $"/usr/bin/brook wsserver --listen :{Settings.Port} --password {Settings.Password}";
            }

            if (Settings.BrookType == BrookType.wssserver)
            {
                return $"/usr/bin/brook wssserver --domain {Settings.Domain} --password {Settings.Password}";
            }

            if (Settings.BrookType == BrookType.socks5)
            {
                var ip = IsOnlyIPv6 ? IPv6 : IPv4;
                return $"/usr/bin/brook socks5 --socks5 {ip}:{Settings.Port}";
            }

            return runBrookCmd;
        }

        public void Uninstall()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Progress.Step = "卸载Brook";
                    Progress.Percentage = 0;

                    Progress.Desc = "停止Brook服务";
                    RunCmd("systemctl stop brook");
                    RunCmd("systemctl disable brook");
                    Progress.Percentage = 30;

                    Progress.Desc = "删除Brook相关文件";
                    RunCmd("rm -rf /etc/systemd/system/brook.service");
                    RunCmd("rm -rf /usr/bin/brook");
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
    }
}
