using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace cxvxcv
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string ipadresi = "192.168.2.234";
            Console.WriteLine(ipadresi+ " Adresine Paket Gönderiliyor..");

            while (true)
            {
                TcpClient clientSocket = new TcpClient();
                NetworkStream serverStream = default(NetworkStream);
            
                clientSocket.Connect(ipadresi, 5555);
                serverStream = clientSocket.GetStream();
                byte[] gidecekmesaj = Encoding.ASCII.GetBytes("Gönderilen Anlamsız Mesaj");
                serverStream.Write(gidecekmesaj, 0, gidecekmesaj.Length);
                Thread.Sleep(1000);
                serverStream.Flush();
            }
          
         
            
        }
    }
}