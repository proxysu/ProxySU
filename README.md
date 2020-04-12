# ProxySU
V2ray install tools for windows

学习C#用来练手的小工具。代码写的很菜，大佬勿喷。编译环境Visual Studio 2017  使用WPF界面

可一键安装的模式有：tcp、tcp+http伪装、http2(需要域名)、WebSocket+TLS+Web(需要域名)、mKCP及各种伪装、QUIC及各种伪装。

支持的VPS系统为：  
CentOS 7/8   
Debian 8/9/10(推荐)  
Ubuntu 16.04及以上

(注意：如果系统启用了SELinux且工作在Enforcing模式下时，需要将Enforcing更改为Permissive模式，否则使用WebSocket+TLS+Web时，Caddy的service无法开机启动，这种情形一般出现在Centos7/8中，本程序在安装过程中将自动处理。)

目前已支持生成用于

1>v2ray官方程序配置文件(客户端配置)

2>v2rayN (windows)客户端导入二维码和网址

3>Shadowrocket (ios)导入二维码和网址

4>v2rayNG (Android)导入二维码和网址

（程序中只实现在生成v2rayN的，但是Shadowrocket和v2rayNG都可以导入。所以就偷了个懒，哈！）

## License

[(GPL-V3)](https://raw.githubusercontent.com/proxysu/windows/master/LICENSE)

##exe运行文件下载(随代码更新，可能有bug)

[下载](https://github.com/proxysu/windows/raw/master/ProxySU/bin/Release/Release.zip)

## Requirements

Microsoft [.NET Framework 4.0](https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net40-offline-installer) or higher
