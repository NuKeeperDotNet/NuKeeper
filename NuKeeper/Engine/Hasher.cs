using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace NuKeeper.Engine
{
    public static class Hasher
    {
#pragma warning disable CA5351
        private static MD5 md5 = MD5.Create();

        public static string Hash(string input)
        {
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);
            return BytesToString(hash);
        }

        private static string BytesToString(byte[] bytes)
        {
            var result = new StringBuilder();

            foreach (var b in bytes)
            {
                result.Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }

            return result.ToString();
        }
    }
}
