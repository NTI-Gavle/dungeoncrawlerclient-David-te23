using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Net.WebSockets;

namespace DungeonCrawlerClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 54321);
            TcpClient tcpClient = new TcpClient();

            try
            {
                await tcpClient.ConnectAsync(iPEndPoint);
                Console.WriteLine("Connected to server.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to server. " + e.Message);
                return;
            }

            var stream = tcpClient.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };

            try
            {
                //Inloggnings loop
                string serverMessage = await reader.ReadLineAsync();
                Console.WriteLine(serverMessage);

                serverMessage = await reader.ReadLineAsync();
                Console.WriteLine(serverMessage);

                string command = "";
                while (string.IsNullOrEmpty(command))
                {
                    command = Console.ReadLine();
                    if (string.IsNullOrEmpty(command)) continue;

                    byte[] writeBytes = Encoding.UTF8.GetBytes(command + "\n");
                    await tcpClient.GetStream().WriteAsync(writeBytes, 0, writeBytes.Length);

                    serverMessage = await reader.ReadLineAsync();
                    if (serverMessage != null)
                    {
                        Console.WriteLine(serverMessage);
                    }
                    if (serverMessage.Contains("Invalid command format") || serverMessage.Contains("Unknown action"))
                    {
                        command = "";
                        Console.WriteLine("Please enter a valid 'login <username> <password>' or 'register <username> <password>'");
                    }
                    else if (serverMessage.Contains("Login failed"))
                    {
                        command = "";
                    }
                    else if (serverMessage.Contains("registered successfully"))
                    {
                        command = "";
                    }
                    else if (serverMessage.Contains("User already exists"))
                    {
                        command = "";
                    }
                }
            }
            catch (Exception ex) when (ex is IOException || ex is SocketException || ex is ObjectDisposedException)
            {
                Console.WriteLine("Lost connection during login process. Exiting.");
                tcpClient.Close();
                return;
            }
            CancellationTokenSource cts = new CancellationTokenSource();
            var readTask = Task.Run(async () =>
            {
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        string serverResponse = await reader.ReadLineAsync();
                        if (serverResponse == null) break;
                        Console.WriteLine(serverResponse);
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is SocketException || ex is ObjectDisposedException)
                {
                    if (!cts.Token.IsCancellationRequested)
                    {
                        Console.WriteLine("\nConnection lost to the server abruptly.");
                    }
                }
                finally
                {
                    if(!cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                }
            }, cts.Token);

            try
            {
                while (tcpClient.Connected && !cts.Token.IsCancellationRequested)
                {
                    string input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input)) continue;

                    
                    await writer.WriteLineAsync(input);

                    if (input.Trim().ToLower() == "quit" || input.Trim().ToLower() == "exit") break;
                }
            }
            catch (Exception ex) when (ex is IOException || ex is SocketException || ex is ObjectDisposedException)
            {
                Console.WriteLine("Error writing to server, connection may be lost.");
            }
            finally
            {
                cts.Cancel();
                await readTask;
                tcpClient.Close();
                Console.WriteLine("Disconnected from server. Press any key to close.");
            }
        }
    }
}