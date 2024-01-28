using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace ChatClient
{
    class Program
    {
        static string userName;
        static string password;
        private const string _host = "127.0.0.1";
        private const int _port = 8888;
        static TcpClient client;
        static NetworkStream stream;

        static void Main(string[] args)
        {
            Console.WriteLine("Enter /s to sing in or /l to log in");
            Dictionary<string, string> _userAccounts = new Dictionary<string, string>();
            string userAccountPath = @"C:\Temp\UserAccounts.txt";
            OpenDictionaryFile(_userAccounts, userAccountPath);
            while (true)
            {
                switch (Console.ReadLine())
                {
                    case "/s":
                        while (true)
                        {
                            if (SignIn(_userAccounts, userAccountPath))
                            {
                                Console.WriteLine("Enter /s to sing in or /l to log in");
                                break;
                            }
                        }

                        break;

                    case "/l":
                        while (true)
                        {
                            var lines = File.ReadAllLines(userAccountPath);
                            foreach (var line in lines)
                            {
                                var tokens = line.Split("=");
                                var key = tokens[0].Trim();
                                var value = string.Join("", tokens.Skip(1)).Trim();
                                _userAccounts[key] = value;
                            }
                            Console.WriteLine("Enter your username: ");
                            userName = Console.ReadLine();
                            Console.WriteLine("Enter your password: ");
                            password = Console.ReadLine();
                            if (_userAccounts.ContainsKey(userName) && _userAccounts.Values.Contains(password))
                            {
                                Console.Clear();
                                Connect(userName);
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Wrong login or password, try again!");
                            }
                            

                        }
                        break;

                    default: 
                        Console.WriteLine("Wrong command");
                        break;
                }
            }
        }

        private static Dictionary<string, string> OpenDictionaryFile(Dictionary<string, string> userAccounts, string path)
        {
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var tokens = line.Split("=");
                var key = tokens[0].Trim();
                var value = string.Join("", tokens.Skip(1)).Trim();
                userAccounts[key] = value;
            }
            return userAccounts;
        }

        private static void Connect(string userName)
        {
            client = new TcpClient();
            try
            {
                client.Connect(_host, _port);
                stream = client.GetStream();

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
                Console.WriteLine($"Wellcome, {userName}");
                SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        private static bool SignIn(Dictionary<string, string> _userAccounts, string path)
        {
            Console.WriteLine("Enter your username: ");
            userName = Console.ReadLine();
            Console.WriteLine("Enter your password: ");
            password = Console.ReadLine();
            try
            {
                _userAccounts.Add(userName, password);
                var sb = new StringBuilder();
                foreach (var kv in _userAccounts)
                {
                    sb.AppendLine($"{kv.Key}={kv.Value}");
                }
                var text = sb.ToString();
                File.AppendAllText(path, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine("current username exist, try another");
                return false;
            }
            return true;
        }

        static void SendMessage()
        {
            Console.WriteLine("Enter message: ");

            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; 
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);
                }
                catch
                {
                    Console.WriteLine("Connection lost");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0); 
        }
    }
}