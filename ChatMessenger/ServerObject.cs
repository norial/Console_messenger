using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace ChatServer
{
    public class ServerObject
    {
        static TcpListener tcpListener; 
        List<ClientObject> clients = new List<ClientObject>();
        internal string serverHistoryPath = @"C:\Temp\ServerLog.txt";
        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                clients.Remove(client);
        }
        protected internal void Listen()
        {
            //TODO: Message history
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                File.AppendAllText(serverHistoryPath, $"\n{DateTime.Now.ToString()} Server started. Waiting for connection");
                Console.WriteLine("Server started. Waiting for connection");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal void BroadcastMessage(string message, string id)
        {
            var command = message.Split(' ')
                .FirstOrDefault(cmd => cmd.Contains("/p") || cmd.Contains("/P"));
            if (command == "/p" || command == "/P")
            {
                var uName = message.Split(' ')[2];
                var refactoredMessage = message.Replace("/p", "")
                    .Replace("/P", "")
                    .Trim();
                byte[] data = Encoding.Unicode.GetBytes($"Private message from {refactoredMessage}");
                var clientId = clients.FirstOrDefault(user => user.userName == uName)?.Id;
                for (int i = 0; i < clients.Count; i++)
                {
                    if (clients[i].Id == clientId)
                    {
                        clients[i].Stream.Write(data, 0, data.Length);
                    }
                }
            }
            else
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                for (int i = 0; i < clients.Count; i++)
                {
                    if (clients[i].Id != id)
                    {
                        clients[i].Stream.Write(data, 0, data.Length);
                    }
                }
            }
        }

        protected internal void Disconnect()
        {
            tcpListener.Stop(); 

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); 
            }
            Environment.Exit(0); 
        }
    }
}