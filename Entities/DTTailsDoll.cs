// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.DTTailsDoll
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Data;
using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;
using ExeNet;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable
namespace DisasterServer.Entities
{
  public class DTTailsDoll : Entity
  {
    private int _target = -1;
    private int _timer;

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      this.FindSpot(server);
      return new TcpPacket(PacketType.SERVER_DTTAILSDOLL_STATE, new object[4]
      {
        (object) (byte) 0,
        (object) (ushort) this.X,
        (object) (ushort) this.Y,
        (object) (this._target != -1 ? 0 : (this._timer <= 0 ? 1 : 0))
      });
    }

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (this._timer > 0)
      {
        if (this._timer == 1)
          server.TCPSend((TcpSession) server.GetSession((ushort) this._target), new TcpPacket(PacketType.SERVER_DTTAILSDOLL_STATE, new object[1]
          {
            (object) (byte) 3
          }));
        --this._timer;
      }
      lock (server.Peers)
      {
        if (this._timer <= 0 && (this._target == -1 || !server.Peers.ContainsKey((ushort) this._target)))
        {
          foreach (Peer peer in server.Peers.Values)
          {
            if (peer.Player.Character != Character.Exe && peer.Player.RevivalTimes < 2 && !peer.Waiting && !peer.Player.HasEscaped && Ext.Dist((double) peer.Player.X, (double) peer.Player.Y, (double) this.X, (double) this.Y) < 130.0)
            {
              this._target = (int) peer.ID;
              this._timer = 60;
              server.TCPSend((TcpSession) server.GetSession(peer.ID), new TcpPacket(PacketType.SERVER_DTTAILSDOLL_STATE, new object[1]
              {
                (object) (byte) 2
              }));
              break;
            }
          }
        }
        if (this._target != -1)
        {
          if (this._timer <= 0)
          {
            foreach (Peer peer in server.Peers.Values)
            {
              if (peer.Player.Character != Character.Exe && peer.Player.RevivalTimes < 2 && !peer.Waiting && Ext.Dist((double) peer.Player.X, (double) peer.Player.Y, (double) this.X, (double) this.Y) < 80.0)
              {
                this._target = (int) peer.ID;
                break;
              }
            }
            if (!server.Peers.ContainsKey((ushort) this._target))
            {
              this.FindSpot(server);
              this._target = -1;
            }
            else
            {
              Player player = server.Peers[(ushort) this._target].Player;
              if (player.HasEscaped)
              {
                this.FindSpot(server);
                this._target = -1;
              }
              else
              {
                if ((int) Math.Abs(player.X - (float) this.X) >= 4)
                  this.X += Math.Sign((int) player.X - this.X) * 4;
                if ((int) Math.Abs(player.Y - (float) this.Y) >= 2)
                  this.Y += Math.Sign((int) player.Y - this.Y) * 2;
                if (Ext.Dist((double) player.X, (double) player.Y, (double) this.X, (double) this.Y) < 18.0)
                {
                  server.TCPSend((TcpSession) server.GetSession((ushort) this._target), new TcpPacket(PacketType.SERVER_DTTAILSDOLL_STATE, new object[1]
                  {
                    (object) (byte) 1
                  }));
                  this.FindSpot(server);
                  this._target = -1;
                }
              }
            }
          }
        }
      }
      byte num = 0;
      if (this._target == -1)
        num = (byte) 0;
      if (this._target != -1 && this._timer > 0)
        num = (byte) 1;
      if (this._target != -1 && this._timer <= 0)
        num = (byte) 2;
      return new UdpPacket(PacketType.SERVER_DTTAILSDOLL_STATE, new object[3]
      {
        (object) (ushort) this.X,
        (object) (ushort) this.Y,
        (object) num
      });
    }

    private void FindSpot(Server server)
    {
      Vector2[] vector2Array = new Vector2[11]
      {
        new Vector2(177, 944),
        new Vector2(1953, 544),
        new Vector2(3279, 224),
        new Vector2(4101, 544),
        new Vector2(4060, 1264),
        new Vector2(3805, 1824),
        new Vector2(2562, 1584),
        new Vector2(515, 1824),
        new Vector2(2115, 1056),
        new Vector2(984, 1184),
        new Vector2(1498, 1504)
      };
      List<Vector2> vector2List = new List<Vector2>();
      foreach (Vector2 vector2 in vector2Array)
        vector2List.Add(vector2);
      lock (server.Peers)
      {
        foreach (Vector2 vector2 in vector2Array)
        {
          foreach (Peer peer in server.Peers.Values)
          {
            if (Ext.Dist((double) peer.Player.X, (double) peer.Player.Y, (double) vector2.X, (double) vector2.Y) < 480.0)
            {
              vector2List.Remove(vector2);
              break;
            }
          }
        }
        if (vector2List.Count > 0)
        {
          Vector2 vector2 = vector2List[new Random().Next(vector2List.Count)];
          this.X = vector2.X;
          this.Y = vector2.Y;
          DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 2);
          interpolatedStringHandler.AppendLiteral("Tails doll found spot at (");
          interpolatedStringHandler.AppendFormatted<int>(vector2.X);
          interpolatedStringHandler.AppendLiteral(", ");
          interpolatedStringHandler.AppendFormatted<int>(vector2.Y);
          interpolatedStringHandler.AppendLiteral(")");
          Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
        }
        else
        {
          Vector2 vector2 = vector2Array[new Random().Next(vector2List.Count)];
          this.X = vector2.X;
          this.Y = vector2.Y;
          DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 2);
          interpolatedStringHandler.AppendLiteral("Tails doll didn't find a spot, using (");
          interpolatedStringHandler.AppendFormatted<int>(vector2.X);
          interpolatedStringHandler.AppendLiteral(", ");
          interpolatedStringHandler.AppendFormatted<int>(vector2.Y);
          interpolatedStringHandler.AppendLiteral(")");
          Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
        }
      }
    }
  }
}
