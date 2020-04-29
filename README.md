# ProxySU
V2ray and Trojan install tools for windows。V2ray和Trojan一键安装工具。

学习C#用来练手的小工具。代码写的很菜，大佬勿喷。编译环境Visual Studio 2017  使用WPF界面  
* 可一键安装V2ray、Trojan、NaiveProxy，后续还会再添加其他。

##### V2ray可一键安装的模式有：
* tcp 
* tcp+http伪装  
* tcp+TLS 
* tcp+TLS （自签证书）
* WebSocket
* WebSocket+TLS 
* WebSocket+TLS+Web 
* WebSocket+TLS（自签证书） 
* http2  
* http2+TLS+Web
* http2（自签证书）
* mKCP及各种伪装 
* QUIC及各种伪装。  
注：mKCP和QUIC模式使用udp协议，可以有效减少网络延时，有加速的作用，但在网络管控严厉时期，会导致IP被封，我遇到的一次，就是刚安装好，使用了3个小时后，IP被封。以上模式最推荐的是WebSocket+TLS+Web 和http2+TLS+Web 需要有一个域名。如果能加上CDN则稳定性更好。加上CDN后，是加速还是减速，与线路有关。

##### Trojan 可一键安装的模式有：  
* Trojan + TLS + Web

##### NaiveProxy一键安装：  
* NaiveProxy + TLS +Web

支持的VPS系统为：  
* CentOS 7/8   
* Debian 8/9/10 (推荐)  
* Ubuntu 16.04及以上

(注意：如果系统启用了SELinux且工作在Enforcing模式下时，需要将Enforcing更改为Permissive模式，否则使用WebSocket+TLS+Web时，Caddy的service无法开机启动，这种情形一般出现在Centos7/8中，程序在安装过程中将自动处理。)

###### V2ray模式目前已支持生成用于

* [v2ray官方程序](https://www.v2ray.com/chapter_00/install.html)配置文件(客户端配置)  
* [v2rayN (windows)](https://github.com/2dust/v2rayN/releases)客户端导入二维码和网址  
* [Shadowrocket (ios)](https://apps.apple.com/us/app/shadowrocket/id932747118)导入二维码和网址  
* [v2rayNG (Android)](https://github.com/2dust/v2rayNG/releases)导入二维码和网址  

（程序中只实现生成v2rayN的，但是Shadowrocket和v2rayNG都可以导入。）

###### Trojan模式目前已支持生成用于  

* [Trojan官方程序](https://github.com/trojan-gfw/trojan)配置文件（客户端配置）  
* [Trojan-QT5 (windows)](https://github.com/TheWanderingCoel/Trojan-Qt5/releases)导入二维码和网址  
* [Shadowrocket (ios)](https://apps.apple.com/us/app/shadowrocket/id932747118)导入二维码和网址  
* [igniter（Android）](https://github.com/trojan-gfw/igniter/releases)导入二维码和网址  

###### NaiveProxy只支持生成用于：

* [NaiveProxy官方客户端](https://github.com/klzgrad/naiveproxy)配置文件（客户端配置）

## 程序工作流程：  
1. 使用[SSH.NET](https://github.com/sshnet/SSH.NET)登录远程主机  
2.   
* 选择V2ray，则调用V2ray官方安装脚本 `curl -o /tmp/go.sh https://install.direct/go.sh` `bash /tmp/go.sh -f` ，安装V2ray。  
* 选择Trojan，则调用Trojan官方安装脚本 `curl -o /tmp/trojan-quickstart.sh https://raw.githubusercontent.com/trojan-gfw/trojan-quickstart/master/trojan-quickstart.sh` `yes | bash /tmp/trojan-quickstart.sh` 安装Trojan。
3. 根据选择读取相应配置模板，调用[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)生成相应配置文件，并上传到服务器。  
4. 如果使用WebSocket+TLS+Web/http2+TLS+Web/Trojan + TLS + Web模式，则调用Caddy官方安装脚本 `curl https://getcaddy.com -o getcaddy`   
与 `bash getcaddy personal hook.service` ，安装Caddy。  
5. 如果使用Http2/tcp+TLS/WebSocket+TLS模式，则调用  `curl https://raw.githubusercontent.com/acmesh-official/acme.sh/master/acme.sh  | INSTALLONLINE=1  sh` 安装acme.sh，使用acme.sh申请并安装证书到V2ray.  
6. 安装成功后，使用[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)生成兼容于v2rayN的json文件，用C#内置的Base64库将json生成url链接，使用[QRcoder](https://github.com/codebude/QRCoder)生成二维码。

* 注：V2ray安装及配置文件主要参考自：  
[V2ray官网](https://www.v2ray.com)  
[白话文教程](https://toutyrater.github.io/)  
[新白话文教程(社区版)](https://guide.v2fly.org/)

* 注：Trojan安装及配置文件主要参考自：  
[Trojan官方配置文档](https://trojan-gfw.github.io/trojan/config)  
[Trojan官方安装说明](https://github.com/trojan-gfw/trojan/wiki/Binary-&-Package-Distributions)  
[自建梯子教程-Trojan](https://trojan-tutor.github.io/2019/04/10/p41.html)

## License

[(GPL-V3)](https://raw.githubusercontent.com/proxysu/windows/master/LICENSE)

## 运行文件下载
* Bete版(随代码更新，新添加功能可能有bug或不完善)  
[下载](https://github.com/proxysu/windows/raw/master/ProxySU/bin/Release/Release.zip)
* 正式版（正式发布的版本，新功能完善后发布）  
[下载](https://github.com/proxysu/windows/releases)

## Windows系统需要安装net4.0及以上

Microsoft [.NET Framework 4.0](https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net40-offline-installer) or higher

## 使用的C# 库  
[SSH.NET --------------- https://github.com/sshnet/SSH.NET](https://github.com/sshnet/SSH.NET)  
[Newtonsoft.Json ------ https://github.com/JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)  
[QRcoder --------------- https://github.com/codebude/QRCoder](https://github.com/codebude/QRCoder)

## 程序安全  
为了布署方便，程序使用root账户登录主机，出于慎重，请不要在运行重要程序及用于生产的主机上使用。程序所有源码开源，所使用的库都是github开源项目，可以保障最大的使用安全，程序不夹带任何私货及后门，不是在本项目地址下载的，不做任何保障，请尽可能从本项目地址下载。

## 程序使用问题反馈  
* Telegram群组 https://t.me/proxysuissues  
* 在线提问  https://github.com/proxysu/windows/issues  
* 邮箱反馈  proxysetuptools@gmail.com

## v1.0.0发布小记  
  足足用了近两个月的业余时间，终于做成一个功能还算完善的版本。虽是一个简单的小工具，没想到对于我这个初学C#的人，还是有点小吃力，如果不是因为武汉肺炎疫情，被禁足在家，还真没时间。学习C#，为啥编写这样一个小工具软件来练手？现在一键安装脚本多的是，这样的工具还有必要吗？咋说呢？我也不知道有多少人会喜欢这个小工具，只是觉得自己用着方便，也想方便一下别人吧，喜欢用就用，不喜欢，也随意。  
  生意又忙起来了，对于我这个业余的编程爱好者，可能没有多少业余时间继续折腾了，尽力吧。  
  （记于2020.4.18）

