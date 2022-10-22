using System;
using System.Security.Cryptography;
using System.Text;

namespace FuckSunrun.Services.Sunrun
{
    public class Crypto
    {
        private readonly Random _random = new();

        protected internal string EncryptKey()
        {
            var str1 = "abcdefghijklmnopqrstuvwxyz";
            var str2 = "";
            for (var i = 0; i < 10; i++)
            {
                var j = _random.Next(str1.Length);
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(str2);
                stringBuilder.Append(str1[j]);
                str2 = stringBuilder.ToString();
                stringBuilder = new StringBuilder();
                stringBuilder.Append(str1[j]);
                str1 = str1.Replace(stringBuilder.ToString(), "");
            }

            return str2;
        }

        protected internal string Encrypt(string key, int enc)
        {
            if (key.Length != 10) throw new Exception("阳光加密密钥错误");
            var stringBuffer = new StringBuilder();
            foreach (var num in enc.ToString())
                stringBuffer.Append(key[num - "0".ToCharArray()[0]]);
            return stringBuffer.ToString();
        }

        protected internal string UserInfoEncrypt()
        {
            string str1;
            string str2;
            var dateTime = DateTime.Now;
            var i = dateTime.Minute;
            var j = dateTime.Second;
            if (i < 10)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("0");
                stringBuilder.Append(i);
                str1 = stringBuilder.ToString();
            }
            else
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(i);
                str1 = stringBuilder.ToString();
            }

            if (j < 10)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("0");
                stringBuilder.Append(j);
                str2 = stringBuilder.ToString();
            }
            else
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(j);
                str2 = stringBuilder.ToString();
            }

            var stringBuilder1 = new StringBuilder();
            stringBuilder1.Append("A");
            var stringBuilder2 = new StringBuilder();
            stringBuilder2.Append(str1);
            stringBuilder2.Append(str2);
            stringBuilder2.Append(string.Empty);
            var rDel = Aes.Create();
            rDel.Key = Encoding.UTF8.GetBytes("osldaaasmkldospd");
            rDel.IV = Encoding.UTF8.GetBytes("0392030003920392");
            rDel.Mode = CipherMode.CBC;
            rDel.Padding = PaddingMode.PKCS7;
            var cTransform = rDel.CreateEncryptor();
            var data = Encoding.UTF8.GetBytes(stringBuilder2.ToString());
            var result = cTransform.TransformFinalBlock(data, 0, data.Length);
            return Convert.ToBase64String(result, 0, result.Length);
        }
    }
}

