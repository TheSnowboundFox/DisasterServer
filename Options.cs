using Microsoft.CSharp.RuntimeBinder;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DisasterServer
{
    public class Options
    {
        private static JsonNode? _doc;
        private static string _path = "Options.json";

        static Options()
        {
            try
            {
                if (Environment.GetCommandLineArgs().Length > 1)
                    _path = Environment.GetCommandLineArgs()[1];
                if (!File.Exists(_path))
                    WriteDefault();
                _doc = JsonNode.Parse(File.ReadAllText(_path));
            }
            catch
            {
                Terminal.Log("Failed to load config.");
            }
        }

        private static void WriteDefault()
        {
            try
            {
                string str = JsonSerializer.Serialize(new
                {
                    debug_mode = true
                });
                _doc = JsonNode.Parse(str);
                File.WriteAllText(_path, str);
            }
            catch
            {
                Terminal.Log("Failed to save config.");
            }
        }

        public static T Get<T>(string key, T defaultValue = default)
        {
            if (_doc == null)
                throw new InvalidOperationException("Configuration document is not loaded.");

            JsonNode? node = _doc[key];
            return node == null ? defaultValue : node.Deserialize<T>();
        }
    }
}
