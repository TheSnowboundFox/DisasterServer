// Decompiled with JetBrains decompiler
// Type: DisasterServer.Options
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using Microsoft.CSharp.RuntimeBinder;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

#nullable enable
namespace DisasterServer
{
  public class Options
  {
    private static JsonNode _doc;
    private static string _path = "Options.json";

    static Options()
    {
      try
      {
        if (Environment.GetCommandLineArgs().Length > 1)
          Options._path = Environment.GetCommandLineArgs()[1];
        if (!File.Exists(Options._path))
          Options.WriteDefault();
        Options._doc = JsonNode.Parse(File.ReadAllText(Options._path));
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
          debug_mode = false
        });
        Options._doc = JsonNode.Parse(str);
        File.WriteAllText(Options._path, str);
      }
      catch
      {
        Terminal.Log("Failed to save config.");
      }
    }
    public static T? Get<T>(string key) => Options._doc[key].AsValue().Deserialize<T>();
  }
}
