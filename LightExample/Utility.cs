using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LightExample
{
    public static class Utility
    {
        // https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Signature(string payload, string secret)
        {
            var hmac256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac256.ComputeHash(Encoding.UTF8.GetBytes(payload));

            return Convert.ToBase64String(hash);
        }
    }
}
