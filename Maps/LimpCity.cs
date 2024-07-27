// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.LimpCity
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Entities;
using DisasterServer.Session;
using ExeNet;
using System.IO;

#nullable enable
namespace DisasterServer.Maps
{
  public class LimpCity : Map
  {
    public override void Init(Server server)
    {
      this.SetTime(server, 155);
      this.Spawn<LCEye>(server, new LCEye()
      {
        ID = (byte) 0
      });
      this.Spawn<LCEye>(server, new LCEye()
      {
        ID = (byte) 1
      });
      this.Spawn<LCChainController>(server);
      base.Init(server);
    }

    public override void Tick(Server server) => base.Tick(server);

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      reader.ReadBoolean();
      if (reader.ReadByte() == (byte) 81)
      {
        bool flag = reader.ReadBoolean();
        byte index = reader.ReadByte();
        byte num = reader.ReadByte();
        lock (this.Entities)
        {
          LCEye[] ofType = this.FindOfType<LCEye>();
          if (ofType != null)
          {
            if ((int) index < ofType.Length)
            {
              LCEye lcEye = ofType[(int) index];
              if (flag)
              {
                if (!lcEye.Used)
                {
                  if (lcEye.Charge >= 20)
                  {
                    lcEye.UseID = session.ID;
                    lcEye.Target = num;
                    lcEye.Used = true;
                    lcEye.SendState(server);
                  }
                }
              }
              else
              {
                lcEye.Used = false;
                lcEye.SendState(server);
              }
            }
          }
        }
      }
      base.PeerTCPMessage(server, session, reader);
    }

    protected override int GetRingSpawnCount() => 23;
  }
}
