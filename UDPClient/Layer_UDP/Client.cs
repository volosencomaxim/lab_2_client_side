using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPClient
{
    class Client
    {

        private UdpClient client;
        private IPEndPoint endPoint;

        private int retransmissionsLimit = 5;

        private byte[] buffer;
        private byte[] serverBuffer;

        public Client()
        {

            client = new UdpClient(Service.clientPort);
            endPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        public void SendRequest(string input)
        {
            var message = RefactorUserInput(input);

            buffer = Encoding.Unicode.GetBytes(message);

            client.Send(buffer, buffer.Length, Service.hostName, Service.serverPort);

            CheckResponse();
        }

        public string ReceiveResponse()
        {
            buffer = client.Receive(ref endPoint);

            string clientMessage = Encoding.Unicode.GetString(buffer);

            if (clientMessage != "nack" && clientMessage != "ack")
            {

                bool isValid = MessageChecker(clientMessage);

                if (!isValid)
                    ResendMessage();

                if (isValid)
                    SendAck();
                return clientMessage;

            }
            else
            {
                return "none";
            }

        }

        private bool MessageChecker(string clientMessage)
        {

            if (string.IsNullOrEmpty(clientMessage))
                return false;
            JObject json = JObject.Parse(clientMessage);

            int clientHash = int.Parse(json["hash"].ToString());

            int hash = json["message"].ToString().GetHashCode();

            if (clientHash != hash)
                return false;

            //Console.WriteLine("** message : " + json["message"].ToString());

            return true;
        }
        private void ResendMessage()
        {
            buffer = Encoding.Unicode.GetBytes("nack");

            client.Send(buffer, buffer.Length, Service.hostName, Service.clientPort);
        }

        private void SendAck()
        {
            buffer = Encoding.Unicode.GetBytes("ack");

            client.Send(buffer, buffer.Length, Service.hostName, Service.serverPort);
        }
        private string RefactorUserInput(string clientInput)
        {
            var hash = clientInput.GetHashCode();

            string message = "{\"hash\":" + $"\"{hash}\"," +
                                    $"\"message\": \"{clientInput}\"" + "}";
            return message;
        }

        private void CheckResponse()
        {
            serverBuffer = client.Receive(ref endPoint);

            CheckMessageValidation(serverBuffer, buffer);
        }
        private void CheckMessageValidation(byte[] serverBuffer, byte[] buffer)
        {
            var message = Encoding.Unicode.GetString(serverBuffer);

            for (int counter = 0; counter < retransmissionsLimit; counter++)
            {
                if (message == "nack")
                {
                    client.Send(buffer, buffer.Length, Service.hostName, Service.serverPort);
                    //Console.WriteLine("error");
                }
                if (message == "ack")
                {
                    //Console.WriteLine("Good Requst");
                    break;
                }
            }
        }
    }
}
