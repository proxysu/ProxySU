# ProxySU

V2ray, Xray, Trojan, NaiveProxy, Trojan-Go, MTProto Go, Brook, Sing-Box, WireGuard, Hysteria 一键安装工具 for Windows。

支持协议：
- **Xray/V2ray**: VLESS, VMess, Trojan, Shadowsocks (AEAD/2022)
- **Xray 扩展协议**: VLESS XTCP, VLESS XTLS Vision, VLESS XHTTP, VLESS REALITY
- **Sing-Box**: VLESS, VMess, Trojan, Shadowsocks, Shadowsocks 2022, WireGuard, Hysteria, Hysteria2, TUIC, NaiveProxy, ShadowTLS, SOCKS, HTTP
- **其他**: Brook, NaiveProxy, Trojan-Go, MTProto Go

编译环境 Visual Studio 2022，使用 WPF 界面。

![photo_2021-05-31_17-23-29](https://user-images.githubusercontent.com/73510229/120171754-f46ffd00-c234-11eb-8105-4e6a941a65bb.jpg)
![photo_2021-05-31_17-24-29](https://user-images.githubusercontent.com/73510229/120171966-297c4f80-c235-11eb-921a-2ddebad5dc59.jpg)


#### 免责声明
ProxySU 属于自用分享工具，请勿用于违背良知与道德之事，否则后果自负。

#### 再次声明
ProxySU 本着技术中立的原则，没有任何立场，也不持任何见解，更不涉及任何政治因素。ProxySU 仅仅主张人的知情权，这是一项天赋人权，也是各国宪法所保障的最基本人权。

##### 支持的 VPS 系统为：
- CentOS 7/8
- Debian 9/10/11/12 (推荐 10+)
- Ubuntu 18 及以上

(注意：如果系统启用了 SELinux 且工作在 Enforcing 模式下时，需要将 Enforcing 更改为 Permissive 模式，否则使用 WebSocket+TLS+Web 时，Caddy 的 service 无法开机启动，这种情形一般出现在 Centos7/8 中，程序在安装过程中将自动处理。)

##### ProxySU 使用教程
[一键搭建科学上网工具 ProxySU](https://github.com/Alvin9999/new-pac/wiki/%E4%B8%80%E9%94%AE%E6%90%AD%E5%BB%BA%E7%A7%91%E5%AD%A6%E4%B8%8A%E7%BD%91%E5%B7%A5%E5%85%B7ProxySU)------------网友 [Alvin9999](https://github.com/Alvin9999) 制作。

##### 使用提醒及常见问题
有些 vps 主机商会对 vps 所使用的资源进行限制，可能会造成 Caddy 启动失败，提示 "failed to create new OS thread" 错误，在一些免费 vps 类型上经常出现。可以切换为不使用 web 伪装的代理模式（不填伪装网址的就是了）。

vps 主机配置推荐内存在 256M 及以上，过低的内存配置，可能导致某些代理方案不能成功运行。

纯 IPV6 主机，安装过程中，将临时设置 NAT64 网关。仅用于布署代理，布署完成后，则会删除，若设置伪装网站，该网址需要可以使用 ipv6 访问，否则将无效。

ProxySU 的安装流程，是假设在全新系统下，没有装过以上代理软件，如果已经安装过，最好将系统重装一下，会减少很多的麻烦。

ProxySU 将安装代理软件的最新版本，为了最好的兼容，请确保客户端也是当前最新版本。

##### 关于兼容的密钥格式
ProxySU 调用 [SSH.NET](https://github.com/sshnet/SSH.NET) 库登录远程主机，目前 SSH.NET 只支持以下格式的密钥：
- RSA in OpenSSL PEM and ssh.com format
- DSA in OpenSSL PEM and ssh.com format
- ECDSA 256/384/521 in OpenSSL PEM format
- ED25519 in OpenSSH key format

##### 关于 Let's Encrypt 证书
ProxySU 所使用的 acme.sh 与 Caddy，都是申请的 Let's Encrypt 免费证书。三个月需要续期，都是自动完成续期的，无需用户操作。

## License

[GPL-V3](https://raw.githubusercontent.com/proxysu/windows/master/LICENSE)

## 运行文件下载
* Beta 版（随代码更新，新添加功能可能有 bug 或不完善）
