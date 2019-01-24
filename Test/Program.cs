using Gravitybox.CommonUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Encrypt1();
        }

        private static void Encrypt1()
        {
            try
            {
                var key = "9999999999999999";
                var iv = "jjjjjjjj";
                var result = SecurityUtilities.Encrypt("qqq", key, iv);
                var plain = SecurityUtilities.Decrypt(result, key, iv);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
