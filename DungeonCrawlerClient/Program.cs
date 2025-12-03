using System.Net.Sockets;
using System.Net;
using System.Text;

namespace DungeonCrawlerClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 54321);
            TcpClient tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(iPEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to server.");
            }

            while (tcpClient.Connected)
            {
                byte[] bytes = new byte[1024];
                tcpClient.GetStream().Read(bytes, 0, bytes.Length);
                string message = Encoding.UTF8.GetString(bytes);
                Console.Write(message);

                string command = Console.ReadLine();
                byte[] writeBytes = Encoding.UTF8.GetBytes(command);
                tcpClient.GetStream().Write(writeBytes, 0, writeBytes.Length);

            }

            tcpClient.Close();

        }
    }
}