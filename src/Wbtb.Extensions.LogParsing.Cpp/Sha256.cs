using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Wbtb.Extensions.LogParsing.Cpp
{
    internal class Sha256
    {
        /// <summary>
        /// Generates a SHA256 hash from a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FromString(string str)
        {
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(str));
            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(stream);
                return ToHex(hash);
            }
        }

        private static string ToHex(byte[] bytes)
        {
            StringBuilder s = new StringBuilder();

            foreach (byte b in bytes)
                s.Append(b.ToString("x2").ToLower());

            return s.ToString();
        }
    }
}
