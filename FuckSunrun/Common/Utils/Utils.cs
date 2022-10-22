using System;
using System.Text;

namespace FuckSunrun.Common.Utils
{
    public static class Utils
    {
        public static string GenerateRandomString(int length)
        {
            var str = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str.Append(ch);
            }
            return str.ToString();
        }
    }
}

