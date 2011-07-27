using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace SecureSocketsLayer.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
               // MicrosoftSslStreamTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            try
            {
                CustomSslStreamTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void MicrosoftSslStreamTest()
        {
            IPAddress hostadd = Dns.GetHostEntry("localhost").AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(hostadd, 4433);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(endPoint);

            System.Net.Security.SslStream ssl = new System.Net.Security.SslStream(new NetworkStream(socket, true));
            ssl.AuthenticateAsClient("localhost");
            ssl.Close();
        }

        static void CustomSslStreamTest()
        {
            IPAddress hostadd = Dns.GetHostEntry("localhost").AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(hostadd, 4433);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(endPoint);

            SecureSocketLayer.Net.Security.RemoteCertificateValidationCallback serverCallback = new SecureSocketLayer.Net.Security.RemoteCertificateValidationCallback(CertificateValidation);

            SecureSocketLayer.Net.Security.SslStream ssl = new SecureSocketLayer.Net.Security.SslStream(new NetworkStream(socket, true), false, serverCallback);
            ssl.AuthenticateAsClient("localhost");
            ssl.Close();
        }

        static bool CertificateValidation(
            object sender, 
            X509Certificate certificate, 
            X509Chain chain, 
            SecureSocketLayer.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}