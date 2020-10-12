using System;
using System.Security.Cryptography;
using System.Text;
using Sinric.json;

namespace Sinric
{
    public static class Utility
    {
        // https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Signature(string payload, string secret)
        {
            var hmac256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac256.ComputeHash(Encoding.UTF8.GetBytes(payload));

            return Convert.ToBase64String(hash);
        }

        public static bool ValidateSignature(SinricMessage message, string secretKey)
        {
            var payloadString = message.RawPayload?.Value as string;

            if (!string.IsNullOrEmpty(payloadString))
            {
                // if the message contains a payload then we need to validate its signature

                // todo validate timestamp of message, must be within X seconds of local clock, and must be > than the last message time received to protect against replay attacks

                // compute a local signature from the raw payload using our secret key:
                var signature = Utility.Signature(payloadString, secretKey);

                // compare the locally computed signature with the one supplied in the message:
                return signature == message.Signature.Hmac;
            }

            return true;
        }
    }
}
