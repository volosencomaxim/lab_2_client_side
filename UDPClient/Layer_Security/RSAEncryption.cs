using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UDPClient
{
    public class RSAEncryption
    {
        private static RSACryptoServiceProvider csp = new RSACryptoServiceProvider();

        private RSAParameters _privateKey;
        private RSAParameters _publicKey;

        public RSAEncryption()
        {
            _privateKey = csp.ExportParameters(true);
            _publicKey = csp.ExportParameters(false);
        }

        public string PublicKeyString()
        {
            var sw = new StringWriter();
            var xs = new XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, _publicKey);

            Regex expoReg = new Regex("<Exponent>(.*)</Exponent>");
            var exponentMatch = expoReg.Match(sw.ToString());
            string exponent = exponentMatch.Groups[1].ToString();

            Regex moduluReg = new Regex("<Modulus>(.*)</Modulus>");
            var moduluMatch = moduluReg.Match(sw.ToString());
            string mod = moduluMatch.Groups[1].ToString();

            string publicKey = mod + "." + exponent;

            return publicKey;
        }

        public RSAParameters SetPublicKey(string modulus, string exponent)
        {
            RSAParameters result = new RSAParameters();
            result.Modulus = Convert.FromBase64String(modulus);
            result.Exponent = Convert.FromBase64String(exponent);

            return result;
        }

        public string Encrypt(RSAParameters publicKey, string message)
        {
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(publicKey);

            var data = Encoding.Unicode.GetBytes(message);
            var cypher = csp.Encrypt(data, false);
            return Convert.ToBase64String(cypher);
        }

        public string Decrypt(string cypherText)
        {
            var dataBytes = Convert.FromBase64String(cypherText);
            csp.ImportParameters(_privateKey);
            var message = csp.Decrypt(dataBytes, false);
            return Encoding.Unicode.GetString(message);
        }

    }
}