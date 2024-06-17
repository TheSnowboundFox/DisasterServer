using DisasterServer.Session;
using DisasterServer.Session;
using DisasterServer;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Linq;

namespace DisasterServer
{
    public class BanList
    {
        private static DisasterServer.Ban _list = new DisasterServer.Ban();

        static BanList()
        {
            try
            {
                if (!File.Exists("Config/Banlist.json"))
                    WriteDefault();
                else
                    _list = JsonSerializer.Deserialize<DisasterServer.Ban>(File.ReadAllText("Config/Banlist.json"));
            }
            catch
            {
                Terminal.Log("Failed to load banlist.");
            }
        }

        public static void WriteDefault()
        {
            try
            {
                string contents = JsonSerializer.Serialize<DisasterServer.Ban>(_list);
                if (!Directory.Exists("Config"))
                    Directory.CreateDirectory("Config");
                File.WriteAllText("Config/Banlist.json", contents);
            }
            catch
            {
                Terminal.Log("Failed to save banlist.");
            }
        }

        public static bool Ban(string ipAddress)
        {
            foreach (Server server in Program.Servers)
            {
                lock (server.Peers)
                {
                    var peer = server.Peers.Values.FirstOrDefault(p => ((IPEndPoint)p.EndPoint).Address.ToString() == ipAddress);
                    if (peer != null)
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>()
                    {
                        { "ip", ipAddress }
                    };
                        _list.List[ipAddress] = dictionary;
                        File.WriteAllText("Config/Banlist.json", JsonSerializer.Serialize<DisasterServer.Ban>(_list));
                        return true;
                    }
                }
            }
            return false;
        }

        public static void Unban(string nickname)
        {
            if (!_list.List.ContainsKey(nickname))
                return;
            _list.List.Remove(nickname);
            File.WriteAllText("Config/Banlist.json", JsonSerializer.Serialize<DisasterServer.Ban>(_list));
        }

        public static bool Check(string nickname) => _list.List.ContainsKey(nickname);

        public static Dictionary<string, Dictionary<string, string>> GetBanned() => _list.List;
    }
}
