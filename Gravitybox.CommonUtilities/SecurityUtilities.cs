using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Gravitybox.CommonUtilities
{
    public static class SecurityUtilities
    {
        private const string defaultKey = "za8hhs55ws2s432w";
        private const string encryptionIV = "狟⊘㵕ᶷﳰ␍虭";

        static SecurityUtilities()
        {
        }

        /// <summary>
        /// Converts a string to HEX string
        /// </summary>
        public static string ToHexString(string str)
        {
            var bytes = Encoding.Unicode.GetBytes(str);
            return ToHexString(bytes);
        }

        public static string ToHexString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString(); // returns: "48656C6C6F20776F726C64" for "Hello world"
        }

        /// <summary>
        /// Converts a HEX string to normal string
        /// </summary>
        public static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return Encoding.Unicode.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
        }

        public static byte[] FromHexStringToBytes(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        public static string FromHexString(byte[] bytes)
        {
            return FromHexString(Encoding.Unicode.GetString(bytes));
        }

        /// <summary>
        /// Encrypt a string with a specified key
        /// </summary>
        /// <param name="inString">The string to encrypt</param>
        /// <param name="key">The key to use for encryption</param>
        public static string Encrypt(string inString, string key = defaultKey, string iv = encryptionIV)
        {
            if (string.IsNullOrEmpty(key))
                throw new Exception("Invalid key");
            if (string.IsNullOrEmpty(iv))
                throw new Exception("Invalid iv");

            var kArr = Encoding.Unicode.GetBytes(key);
            if (kArr.Length != 32)
                throw new Exception("Invalid key");
            var ivArr = Encoding.Unicode.GetBytes(iv);
            if (ivArr.Length != 16)
                throw new Exception("Invalid iv");

            try
            {
                var aesCSP = new AesCryptoServiceProvider();
                aesCSP.Key = kArr;
                aesCSP.IV = ivArr;
                var encString = EncryptString(aesCSP, inString);
                //return Convert.ToBase64String(encString);
                return ToHexString(encString);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Decrypt a string with a specified key
        /// </summary>
        /// <param name="inString">The string to decrypt</param>
        /// <param name="key">The key to use for decryption</param>
        public static string Decrypt(string inString, string key = defaultKey, string iv = encryptionIV)
        {
            if (string.IsNullOrEmpty(key))
                throw new Exception("Invalid key");
            if (string.IsNullOrEmpty(iv))
                throw new Exception("Invalid iv");

            var kArr = Encoding.Unicode.GetBytes(key);
            if (kArr.Length != 32)
                throw new Exception("Invalid key");
            var ivArr = Encoding.Unicode.GetBytes(iv);
            if (ivArr.Length != 16)
                throw new Exception("Invalid iv");

            try
            {
                var aesCSP = new AesCryptoServiceProvider();
                aesCSP.Key = Encoding.Unicode.GetBytes(key);
                aesCSP.IV = Encoding.Unicode.GetBytes(iv);
                //var encBytes = Convert.FromBase64String(inString);
                var encBytes = FromHexStringToBytes(inString);
                return DecryptString(aesCSP, encBytes);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static byte[] EncryptString(SymmetricAlgorithm symAlg, string inString)
        {
            var inBlock = Encoding.Unicode.GetBytes(inString);
            var xfrm = symAlg.CreateEncryptor();
            var outBlock = xfrm.TransformFinalBlock(inBlock, 0, inBlock.Length);
            return outBlock;
        }

        private static string DecryptString(SymmetricAlgorithm symAlg, byte[] inBytes)
        {
            var xfrm = symAlg.CreateDecryptor();
            var outBlock = xfrm.TransformFinalBlock(inBytes, 0, inBytes.Length);
            return Encoding.Unicode.GetString(outBlock);
        }

        public static string CalculateMD5(string input)
        {
            // step 1, calculate MD5 hash from input
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string MD5Hash(this string input, string entropy = "|z92jg72645sd")
        {
            using (var z = System.Security.Cryptography.MD5.Create())
            {
                var b = System.Text.Encoding.UTF8.GetBytes(input + entropy + "z718hs5");
                var hash = z.ComputeHash(b);
                var sb = new StringBuilder();
                for (var i = 0; i < hash.Length; i++)
                    sb.Append(hash[i].ToString("X2"));
                return sb.ToString();
            }
        }

    }
}
