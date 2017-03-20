using System.Security.Cryptography;
using System.Text;

namespace NetCoreLedger.Business
{
    public static class Hasher
    {
        public static string EmtpySha256 = Sha256(Encoding.UTF8.GetBytes(""));

        public static string Sha256(byte[] data)
        {
            return Sha256(data, 0, data.Length);
        }

        public static string Sha256(byte[] data, int count)
        {
            return Sha256(data, 0, count);
        }

        public static string Sha256(byte[] data, int offset, int count)
        {
            using (var algorithm = SHA256.Create())
            {
                var hash = algorithm.ComputeHash(data, offset, count);
                return GetStringFromHash(hash);
            }
        }

        public static string GetStringFromHash(byte[] hash)
        {
            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}