using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPClient
{
    class ClientEncryption
    {
        private Client _udpClient;
        private RSAEncryption encry;
        private bool _isSecured = false;
        private string publicData;
        private string encryptMessage;

        public ClientEncryption()
        {
            _udpClient = new Client();
            encry = new RSAEncryption();
        }

        public void CreateConnection()
        {
            while (!_isSecured)
            {
                _udpClient.SendRequest(encry.PublicKeyString());

                publicData = _udpClient.ReceiveResponse();

                _isSecured = true;
            }
        }

        public void SendRequest(string input)
        {
            string[] data = ExtractMessage(publicData).Split('.');

            var publicKey = encry.SetPublicKey(data[0], data[1]);

            var encryptedInput = encry.Encrypt(publicKey, input);

            _udpClient.SendRequest(encryptedInput);
        }
        public void ReceiveResponse()
        {
            encryptMessage = _udpClient.ReceiveResponse();
            DecryptMessage();
        }

        private string ExtractMessage(string input)
        {
            JObject json = JObject.Parse(input);

            return json["message"].ToString();
        }
        public void DecryptMessage()
        {
            string decryptMessage = encry.Decrypt(ExtractMessage(encryptMessage));

            Console.WriteLine(decryptMessage);
        }
    }
}
