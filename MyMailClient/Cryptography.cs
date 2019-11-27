using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyMailClient
{
    static class Cryptography
    {
        public const string KEY_DELIVERY_HEADER = "x-MMS-key-delivery-x";

        public static readonly Encoding Enc = Encoding.UTF8;
        public static byte[] GetSHA1(byte[] data)
        {
            byte[] hash;
            using (SHA1Managed sha1 = new SHA1Managed())
                hash = sha1.ComputeHash(data);
            return hash;
        }
        public static byte[] GetSHA1(string data)
        {
            return GetSHA1(Enc.GetBytes(data));
        }
    }
}
