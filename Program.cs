using DisasterServer.Session;
using System.Collections.Generic;
using System.Threading;

namespace DisasterServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Terminal.Log("===================");
            Terminal.Log("DisasterServer");
            Terminal.Log("Build by FoxTheSlav");
            Terminal.Log("===================");
            Terminal.Log("Original files belong to:");
            Terminal.Log("(c) Team Exe Empire 2023");
            Terminal.Log("===================");

            Server server = new Server();
            server.StartAsync();

            // Keep the program running
            while (true)
                Thread.Sleep(1000);
        }
    }
}