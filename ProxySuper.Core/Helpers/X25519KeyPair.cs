using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Chaos.NaCl;

namespace ProxySuper.Core.Helpers
{
    public class X25519KeyPair
    {
        public byte[] _PrivateKey { get; private set; }  // 32 bytes
        public byte[] _PublicKey { get; private set; }   // 32 bytes

        public X25519KeyPair()
        {
            _PrivateKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(_PrivateKey); 
            }
            _PublicKey = MontgomeryCurve25519.GetPublicKey(_PrivateKey);
        }

        public X25519KeyPair(byte[] privateKey)
        {
            if (privateKey == null || privateKey.Length != 32)
                throw new ArgumentException("Private key must be 32 bytes.");

            _PrivateKey = privateKey;
            _PublicKey = MontgomeryCurve25519.GetPublicKey(_PrivateKey);
        }

        public static string Base64UrlEncode(byte[] data)
        {
            string base64 = Convert.ToBase64String(data); // 标准Base64
            base64 = base64.TrimEnd('=');                 // 去掉末尾的 '='
            base64 = base64.Replace('+', '-');            // '+' 替换成 '-'
            base64 = base64.Replace('/', '_');            // '/' 替换成 '_'
            return base64;
        }

        public string PrivateKey
        {
            get { return Base64UrlEncode(_PrivateKey); }
        }

        public string PublicKey
        {
            get { return Base64UrlEncode(_PublicKey); }
        }

    }
}
