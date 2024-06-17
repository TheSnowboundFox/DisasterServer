// Decompiled with JetBrains decompiler
// Type: DisasterServer.KickList
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

#nullable enable
namespace DisasterServer
{
  public class KickList
  {
    private static List<KickData> EndPoints = new List<KickData>();
    private static Timer timer = new Timer();

    static KickList()
    {
      KickList.timer.Interval = 30000.0;
      KickList.timer.Elapsed += new ElapsedEventHandler(KickList.Timer_Elapsed);
      KickList.timer.Start();
    }

    private static void Timer_Elapsed(object? sender, ElapsedEventArgs e1)
    {
      KickList.EndPoints.RemoveAll((Predicate<KickData>) (e2 => (DateTime.Now - e2.Since).TotalMinutes >= 1.0));
    }

    public static void Add(string endpoint)
    {
      KickList.EndPoints.Add(new KickData()
      {
        IP = endpoint,
        Since = DateTime.Now
      });
    }

    public static bool Check(string endpoint)
    {
      return KickList.EndPoints.Any<KickData>((Func<KickData, bool>) (e => e.IP == endpoint));
    }
  }
}
