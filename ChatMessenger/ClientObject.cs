﻿using System;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        internal string userName;
        TcpClient client;
        ServerObject server;
        internal string serverHistoryPath = @"C:\Temp\ServerLog.txt";
        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                string message = GetMessage();
                userName = message;

                message = $"{userName} Entered to chat";
                File.AppendAllText(serverHistoryPath, $"\n{DateTime.Now.ToString()} {message}");
                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message);
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = String.Format($"{userName}: {message}");
                        File.AppendAllText(serverHistoryPath, $"\n{DateTime.Now.ToString()} {message}");
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                    }
                    catch
                    {
                        message = String.Format($"{userName}: Left the chat");
                        File.AppendAllText(serverHistoryPath, $"\n{DateTime.Now.ToString()} {message}");
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}