using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace SteamrequestsTest
{
    internal class Rsa
    {
        private byte[] _exponent;
        private byte[] _modulus;
        public string Exponent
        {
            set
            {
                _exponent = StringToByteArray(value);
            }
        }
        public string Modulus
        {
            set 
            { 
                _modulus = StringToByteArray(value);
            }
        }
        public string Encrypt(string data)
        {
            string encrypted;
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            var parameters = new RSAParameters();
            var provider = new RSACryptoServiceProvider();

            parameters.Exponent = _exponent;
            parameters.Modulus = _modulus;

            provider.ImportParameters(parameters);

            return Convert.ToBase64String(provider.Encrypt(byteData,false));
        }

        private byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
