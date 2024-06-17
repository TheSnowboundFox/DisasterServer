using DisasterServer.Data;
using DisasterServer.Session;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace DisasterServer
{
    public class Program
    {
        public static List<Server> Servers { get; private set; } = new List<Server>();

        public static void Main(string[] args)
        {
            Terminal.Log("===================");
            Terminal.Log("DisasterServer");
            Terminal.Log("Build by TheSlavonicFox");
            Terminal.Log("===================");
            Terminal.Log("Original files belong to:");
            Terminal.Log("(c) Team Exe Empire 2023");
            Terminal.Log("===================");

            Server server = new Server();
            server.StartAsync();
            Servers.Add(server);

            // Start a separate thread to listen for console input
            Thread consoleThread = new Thread(new ParameterizedThreadStart(ConsoleInputListener));
            consoleThread.Start(server);

            // Keep the program running
            while (true)
                Thread.Sleep(1000);
        }

        // Method to listen for console input
        private static void ConsoleInputListener(object serverObj)
        {
            Server server = (Server)serverObj;

            while (true)
            {
                // Read the input from the console
                string input = Console.ReadLine();

                // Parse the input and handle commands
                if (!string.IsNullOrWhiteSpace(input))
                {
                    string[] parts = input.Split(' ');
                    string command = parts[0].ToLower();

                    switch (command)
                    {
                        case "ban":
                            if (parts.Length >= 2)
                            {
                                string ipAddress = parts[1];
                                BanList.Ban(ipAddress);
                                Terminal.Log($"Banned IP address {ipAddress}");
                            }
                            else
                            {
                                Terminal.LogCommand("Usage: ban <ip_address>");
                            }
                            break;
                        case "unban":
                            if (parts.Length >= 2)
                            {
                                string ipAddress = parts[1];
                                BanList.Unban(ipAddress);
                                Terminal.Log($"Unbanned IP address {ipAddress}");
                            }
                            else
                            {
                                Terminal.LogCommand("Usage: unban <ip_address>");
                            }
                            break;
                        case "list":
                            // Display all peers
                            Terminal.Log("List of Peers:");
                            Terminal.Log("PID - Nickname - Unique - IP");

                            foreach (var peer in server.Peers.Values)
                            {
                                string ipAddress = ((IPEndPoint)peer.EndPoint).Address.ToString();
                                Terminal.Log($"""{peer.ID} - {peer.Nickname} - "{peer.Unique}" - {ipAddress}""");
                            }
                            break;
                        // Add more commands as needed
                        default:
                            Terminal.LogCommand("Unknown command.");
                            break;
                    }
                }
            }
        }
    }
}