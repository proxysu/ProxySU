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
    public class MTProtoGoService : ServiceBase<MTProtoGoSettings>
    {
        public MTProtoGoService(Host host, MTProtoGoSettings settings) : base(host, settings)
        {
        }

        public void Install()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Progress.Step = "1. 检测系统环境";
                    Progress.Percentage = 0;

                    EnsureRootUser();
                    EnsureSystemEnv();
                    Progress.Percentage = 15;

                    Progress.Step = "2. 安装必要的系统工具";
                    InstallSystemTools();
                    Progress.Percentage = 25;

                    Progress.Step = "3. 配置防火墙";
                    ConfigFirewalld();
                    Progress.Percentage = 35;

                    Progress.Step = "4. 安装docker";
                    InstallDocker();
                    Progress.Percentage = 50;

                    Progress.Step = "5. 生成密钥";
                    //RunCmd("docker pull nineseconds/mtg:2");   //拉取v2版本命令，但是似乎不用也可以。
                    //下面的命令是v1版本的命令格式。可以保留。将来如启用v1，可用。
                    //Settings.SecretText = RunCmd($"docker run nineseconds/mtg:1 generate-secret -c {Settings.Cleartext} tls").TrimEnd('\n');
                    Settings.SecretText = RunCmd($"docker run nineseconds/mtg:2 generate-secret {Settings.Cleartext}").TrimEnd('\n');
                    Progress.Percentage = 65;

                    Progress.Step = "6. 生成配置文件";
                    Progress.Desc = "创建配置";
                    RunCmd("touch /etc/mtg.toml");

                    Progress.Desc = "写入配置内容";
                    RunCmd($"echo \"secret=\\\"{Settings.SecretText}\\\"\" > /etc/mtg.toml");
                    RunCmd($"echo \"bind-to=\\\"0.0.0.0:{Settings.Port}\\\"\" >> /etc/mtg.toml");
                    Progress.Percentage = 80;

                    Progress.Step = "7. 启动MTProto服务";
                    RunCmd($"docker run -d -v /etc/mtg.toml:/config.toml  --name=mtg --restart=always -p {Settings.Port + ":" + Settings.Port} nineseconds/mtg:2");
                    Progress.Desc = "设置自启动MTProto服务";

                    Progress.Step = "安装完成";
                    Progress.Percentage = 100;

                    AppendCommand("Host: " + Settings.Domain);
                    AppendCommand("Port: " + Settings.Port);
                    AppendCommand("Secret: " + Settings.SecretText);

                    NavigationService.Navigate<MTProtoGoConfigViewModel, MTProtoGoSettings>(Settings);
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
                    Progress.Percentage = 0;
                    Progress.Step = "卸载MTProto";

                    Progress.Desc = "检测系统环境";
                    EnsureRootUser();
                    Progress.Percentage = 30;

                    Progress.Desc = "删除docker容器";
                    var cid = RunCmd("docker ps -q --filter name=mtg");
                    RunCmd($"docker stop {cid}");
                    RunCmd($"docker rm {cid}");
                    Progress.Percentage = 100;
                    Progress.Desc = "卸载完成";
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
                    Progress.Percentage = 0;
                    Progress.Step = "更新MTProto配置";

                    Progress.Desc = "暂停MTProto服务";
                    var cid = RunCmd("docker ps -q --filter name=mtg");
                    RunCmd($"docker stop {cid}");
                    Progress.Percentage = 50;

                    Progress.Desc = "生成密钥";
                    //Settings.SecretText = RunCmd($"docker run nineseconds/mtg:1 generate-secret -c {Settings.Cleartext} tls").TrimEnd('\n'); //v1版本
                    Settings.SecretText = RunCmd($"docker run nineseconds/mtg:2 generate-secret {Settings.Cleartext}").TrimEnd('\n');
                    Progress.Percentage = 65;

                    Progress.Desc = "修改配置文件";
                    RunCmd($"echo \"secret=\\\"{Settings.SecretText}\\\"\" > /etc/mtg.toml");
                    RunCmd($"echo \"bind-to=\\\"0.0.0.0:{Settings.Port}\\\"\" >> /etc/mtg.toml");
                    Progress.Percentage = 80;

                    Progress.Desc = "重启MTProto服务";
                    RunCmd($"docker restart {cid}");

                    Progress.Percentage = 100;
                    Progress.Desc = "更新配置成功";

                    AppendCommand("Host: " + Settings.Domain);
                    AppendCommand("Port: " + Settings.Port);
                    AppendCommand("Secret: " + Settings.SecretText);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void InstallDocker()
        {
            Progress.Desc = "执行docker安装脚本";
            RunCmd("yes | curl https://get.docker.com | sh");

            if (!FileExists("/usr/bin/docker"))
            {
                Progress.Desc = "docker安装失败";
                throw new Exception("docker安装失败");
            }
        }
    }
}
