# ProxySU
V2ray, Xray,Trojan, NaiveProxy, Trojan-Go, Brook,BBR install tools for windows。  
V2ray，Xray,Trojan，NaiveProxy, Trojan-Go, Brook 及相关插件。支持纯ipv6主机一键安装代理。  
BBR一键开启（仅支持CentOS8/Debian9/10/Ubuntu18.04及以上）,支持语言:English、简体中文、正体（繁体）中文。

编译环境Visual Studio 2017  使用WPF界面。可一键安装 V2ray/Xray, Shadowsocks, Trojan, Trojan-Go, NaiveProxy, Brook 后续还会再添加其他。  

![photo_2021-05-31_17-23-29](https://user-images.githubusercontent.com/73510229/120171754-f46ffd00-c234-11eb-8105-4e6a941a65bb.jpg)
![photo_2021-05-31_17-24-29](https://user-images.githubusercontent.com/73510229/120171966-297c4f80-c235-11eb-921a-2ddebad5dc58.jpg)



#### 免责声明：ProxySU属于自用分享工具，请勿用于违背良知与道德之事，否则后果自负。  

#### 再次声明：  
##### ProxySU本着技术中立的原则，没有任何立场，也不持任何见解，更不涉及任何政治因素。ProxySU仅仅主张人的知情权，这是一项天赋人权，也是各国宪法所保障的最基本人权。知情权包含对同一事物正负两方面评价的知情，至于相信哪个，由人自己选择。正如李文亮医生临终所言：一个正常的社会是不应该只有一种声音的。如果真的存在对某一事物只有一种声音的评价，无论其评价是正面还是负面，都是要慎重对待，并需要重新审视的。  

##### 上传自有证书 #####
需要将crt和key文件打包成zip，在安装界面选择“上传自有证书”

##### 支持的VPS系统为：  
* CentOS 7/8   
* Debian 8/9/10 (推荐 10)  
* Ubuntu 16.04及以上

(注意：如果系统启用了SELinux且工作在Enforcing模式下时，需要将Enforcing更改为Permissive模式，否则使用WebSocket+TLS+Web时，Caddy的service无法开机启动，这种情形一般出现在Centos7/8中，程序在安装过程中将自动处理。)  

##### ProxySU使用教程  
[一键搭建科学上网工具ProxySU](https://github.com/Alvin9999/new-pac/wiki/%E4%B8%80%E9%94%AE%E6%90%AD%E5%BB%BA%E7%A7%91%E5%AD%A6%E4%B8%8A%E7%BD%91%E5%B7%A5%E5%85%B7ProxySU)------------网友 [Alvin9999](https://github.com/Alvin9999) 制作。    
[Youtube视频教程](https://www.youtube.com/watch?v=ipFZDE1Aqoo)---------------------------网友 [jc-nf那坨](https://www.youtube.com/channel/UC52iA9wBGGN7LBWSdyI-yfg) 制作，需要先翻墙后观看。  

##### 使用提醒及常见问题：  
有些vps主机商会对vps所使用的资源进行限制，可能会造成Caddy启动失败，提示“failed to create new OS thread ”错误，在一些免费vps类型上经常出现。可以切换为不使用web伪装的代理模式（不填伪装网址的就是了）。  

vps主机配置推荐内存在256M及以上，过低的内存配置，可能导致某些代理方案不能成功运行。

纯IPV6主机，安装过程中，将临时设置NAT64网关。仅用于布署代理，布署完成后，则会删除,若设置伪装网站，该网址需要可以使用ipv6访问，否则将无效。注意，纯IPV6 的主机无法直接访问纯IPV4的网络。(不推荐使用纯ipv6主机做为代理节点)  

ProxySU的安装流程，是假设在全新系统下，没有装过以上代理软件，如果已经安装过，最好将系统重装一下，会减少很多的麻烦。  
ProxySU将安装代理软件的最新版本，为了最好的兼容，请确保客户端也是当前最新版本。  

在实际使用中，发现Centos7,debian8,ubunutu16.04等版本，安装出错的机率很大，不建议使用以上版本。低于以上版本的，无法使用ProxySU.  

Nat类型的vps主机，因为无法独占80，443端口，使用tls模式的代理，可能不能申请证书，安装会失败。  

ProxySU在开发过程中，一般都是在vultr的vps中测试，测试系统版本为：Debian 10。由于同一个版本的Linux系统，不同的VPS商,云服务商也不完全相同。实在没有精力去逐一测试。如果安装失败，可以先尝试以下方法解决：  

1.如果以前装过翻墙软件，请重装一下vps系统后，再试。  
2.更换为其他版本的linux发行版，推荐使用 Debian 9与Debian 10系统，再试。  

以上两种方法不能解决，请各位网友及时发[issues](https://github.com/proxysu/windows/issues)，或者到[TG群组](https://t.me/proxysuissues)，反馈。  

在以上平台反馈求助时，请尽可能将安装出错的日志保存下来，并提供，将有利于查找错误原因。  
在反馈问题前，可以先看一下 <<[常见问题集锦](https://github.com/proxysu/windows/wiki/CommonError)>>，是否可以解决。  

##### 关于兼容的密钥格式    
ProxySU调用[SSH.NET](https://github.com/sshnet/SSH.NET)库登录远程主机，目前[SSH.NET](https://github.com/sshnet/SSH.NET)只支持以下格式的密钥：  
* RSA in OpenSSL PEM and ssh.com format  
* DSA in OpenSSL PEM and ssh.com format  
* ECDSA 256/384/521 in OpenSSL PEM format  
* ED25519 in OpenSSH key format  
见官方说明：  
https://github.com/sshnet/SSH.NET#public-key-authentication  
如果当前拥有的密钥格式不是以上几种，可以下载[puttygen](https://www.puttygen.com/)工具，将其转换成上面的格式。  
[puttygen](https://www.puttygen.com/)使用教程可以[看这里](https://github.com/proxysu/windows/wiki/PrivateKeyConversionFormat)

##### 关于Let's Encrypt证书  
ProxySU所使用的acme.sh与Caddy，都是申请的Let's Encrypt免费证书。三个月需要续期，都是自动完成续期的，无需用户操作。但是Let's Encrypt证书有一些限制，请知晓，如下：  

Let's Encrypt证书申请频率的限制

同一个主域名一周之内只能申请50个证书  
每个账号下每个域名每小时申请验证失败的次数为5次  
每周只能创建5个重复的证书，即使是通过不同的账号进行创建  
每个账号同一个IP地址每3小时最多可以创建10个证书  
每个多域名（SAN） SSL证书（不是通配符域名证书）最多只能包含100个子域  
更新证书没有次数的限制，但是更新证书会受到上述重复证书的限制  
如果提示证书申请失败，可以尝试更换域名再试（添加或换不同的二级域名，也算是新域名）  
同一IP地址，在短时间内过于频繁的申请证书，也会被限制，此时更换域名也无法申请成功，只能等待一段时间，或者更换Ip.  
(网友分享)  

##### 关于卸载功能  
有网友要求，可以卸载其他方法安装的，经过考虑，还是不这样做。1，容易引起争议。2，不容易卸载干净，在用ProxySU安装时可能还会出错。所以第一次使用ProxySU建议使用全新系统，如果以前安装过代理程序，请尽可能将系统重装一下，可以减少很多安装的错误和冲突。  

## License

[(GPL-V3)](https://raw.githubusercontent.com/proxysu/windows/master/LICENSE)

## 运行文件下载
* Beta版(随代码更新，新添加功能可能有bug或不完善)  
[下载](https://github.com/proxysu/windows/raw/master/ProxySU/bin/Beta/Beta.zip)
* 正式版（正式发布的版本，新功能完善后发布）  
[下载](https://github.com/proxysu/windows/releases)

## Windows系统需要安装net4.7.2或以上

Microsoft [.NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net472-offline-installer) or higher

## 使用的C# 库  
[SSH.NET --------------- https://github.com/sshnet/SSH.NET](https://github.com/sshnet/SSH.NET)  
[Newtonsoft.Json ------ https://github.com/JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)  
[QRcoder --------------- https://github.com/codebude/QRCoder](https://github.com/codebude/QRCoder)

## 程序安全  
为了布署方便，程序使用root账户登录主机，出于慎重，请不要在运行重要程序及用于生产的主机上使用。程序所有源码开源，所使用的库都是github开源项目，可以保障最大的使用安全，程序不夹带任何私货、恶意代码及后门，也不会收集任何个人资料，不是在本项目地址下载的，不做任何保障，请尽可能从本项目地址下载。

## 程序使用问题反馈  
* Telegram群组 https://t.me/proxysuissues   
* Telegram频道 https://t.me/proxysu_channel 
* 在线提问  https://github.com/proxysu/windows/issues  

在使用遇到问题时，请先看一下[常见问题集锦](https://github.com/proxysu/windows/wiki/CommonError)，如果还不能解决，可以到以上平台询问，个人精力有限，尽力解答。
## v1.0.0发布小记  
  足足用了近两个月的业余时间，终于做成一个功能还算完善的版本。虽是一个简单的小工具，没想到对于我这个初学C#的人，还是有点小吃力，如果不是因为新冠肺炎疫情，被禁足在家，还真没时间。学习C#，为啥编写这样一个小工具软件来练手？现在一键安装脚本多的是，这样的工具还有必要吗？咋说呢？我也不知道有多少人会喜欢这个小工具，只是觉得自己用着方便，也想方便一下别人吧，喜欢用就用，不喜欢，也随意。  
  生意又忙起来了，对于我这个业余的编程爱好者，可能没有多少业余时间继续折腾了，尽力吧。  
  （记于2020.4.18）
## V2.0.0发布小记  
   哎呀吗！增加一个多语言切换，真是麻烦啊，最初就没有想支持多语言（PS:其实是不会`^_^`) 。中途再添加，真的是累啊！整了一个星期才完事。主要代码简单，就是把所有的中文显示信息替换，那真是一个晕！！英文水平太菜，用谷歌翻译做的，凑合着用吧。
## V3.0.0发布小记
    旧版本功能大而全，如果配置需要修改，或操作多个主机，就没办法做了。也总结了开发WPF经验后，尝试着改变一下，结果衍生了一个尝试的版本。
## V4.0.1发布小记
   在V3.x.x版本中，属于尝试性的改进和开发，应大家要求，在v4版本中保留了TrojanGo、NaiveProxy。
