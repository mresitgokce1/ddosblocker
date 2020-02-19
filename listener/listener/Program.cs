using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;

namespace listener
{
    internal class Program
    {
        public static Hashtable clientList = new Hashtable();
        public static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(5555);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();

            Console.WriteLine("Ethernet Kartı Dinleniyor..");
            counter = 0;
            while (true)
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                byte[] gelenveri = new byte[65536];
                string gelenData = null;

                NetworkStream ns = clientSocket.GetStream();
                var size = clientSocket.ReceiveBufferSize;
                ns.Read(gelenveri, 0, (int) clientSocket.ReceiveBufferSize);
                gelenData = Encoding.ASCII.GetString(gelenveri);
              
               Console.WriteLine($"Paket Gönderen IP -> {DateTime.Now}");
                
                
            }
        }
    }
}