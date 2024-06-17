// Decompiled with JetBrains decompiler
// Type: DisasterServer.State.MapVote
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Data;
using DisasterServer.Maps;
using DisasterServer.Session;
using ExeNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

#nullable enable
namespace DisasterServer.State
{
  public class MapVote : DisasterServer.State.State
  {
    public static readonly Type[] Maps = new Type[19]
    {
      typeof (HideAndSeek2),
      typeof (RavineMist),
      typeof (DotDotDot),
      typeof (DesertTown),
      typeof (YouCantRun),
      typeof (LimpCity),
      typeof (NotPerfect),
      typeof (KindAndFair),
      typeof (Act9),
      typeof (NastyParadise),
      typeof (PricelessFreedom),
      typeof (VolcanoValley),
      typeof (GreenHill),
      typeof (MajinForest),
      typeof (AngelIsland),
      typeof (TortureCave),
      typeof (DarkTower),
      typeof (HauntingDream),
      typeof (FartZone)
    };
    public static List<int> Excluded = new List<int>()
    {
      18
    };
    private MapVoteMap[] _votes = new MapVoteMap[3]
    {
      new MapVoteMap(),
      new MapVoteMap(),
      new MapVoteMap()
    };
    private int _timer = 60;
    private int _timerSec = 30;
    private Random _rand = new Random();
    private Dictionary<ushort, bool> _votePeers = new Dictionary<ushort, bool>();

    public override DisasterServer.Session.State AsState() => DisasterServer.Session.State.VOTE;

    public override void Init(Server server)
    {
      List<int> intList = new List<int>();
      int num1 = this._rand.Next(0, MapVote.Maps.Length);
      int num2 = 0;
      for (int index = 0; index < MapVote.Maps.Length; ++index)
      {
        if (!MapVote.Excluded.Contains(index))
          ++num2;
      }
      if (num2 <= 3)
        server.LastMap = -1;
      for (int index = 0; index < (num2 >= this._votes.Length ? this._votes.Length : num2); ++index)
      {
        while (MapVote.Excluded.Contains(num1) || intList.Contains(num1) || num1 == server.LastMap)
          num1 = this._rand.Next(0, MapVote.Maps.Length - 1);
        intList.Add(num1);
      }
      if (num2 < this._votes.Length)
      {
        for (int index = 0; index < this._votes.Length - num2; ++index)
          intList.Add(num1);
      }
      for (int index = 0; index < intList.Count; ++index)
      {
        this._votes[index].Map = Ext.CreateOfType<Map>(MapVote.Maps[intList[index]]) ?? (Map) new HideAndSeek2();
        this._votes[index].MapID = (byte) intList[index];
        this._votes[index].Votes = 0;
      }
      lock (server.Peers)
      {
        foreach (KeyValuePair<ushort, Peer> peer in server.Peers)
        {
          if (peer.Value.Pending)
            server.DisconnectWithReason((TcpSession) server.GetSession(peer.Key), "PacketType.CLIENT_REQUESTED_INFO missing.");
          else if (!peer.Value.Waiting)
            this._votePeers.Add(peer.Key, false);
        }
      }
      TcpPacket packet = new TcpPacket(PacketType.SERVER_VOTE_MAPS, new object[3]
      {
        (object) this._votes[0].MapID,
        (object) this._votes[1].MapID,
        (object) this._votes[2].MapID
      });
      server.TCPMulticast(packet);
    }

    public override void PeerJoined(Server server, TcpSession session, Peer peer)
    {
    }

    public override void PeerLeft(Server server, TcpSession session, Peer peer)
    {
      lock (server.Peers)
      {
        if (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) <= 1)
        {
          server.SetState<Lobby>();
        }
        else
        {
          lock (this._votePeers)
          {
            this._votePeers.Remove(session.ID);
            if (this._votePeers.Count<KeyValuePair<ushort, bool>>((Func<KeyValuePair<ushort, bool>, bool>) (e => !e.Value)) > 0)
              return;
            this.CheckVotes(server);
          }
        }
      }
    }

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      int num1 = reader.ReadBoolean() ? 1 : 0;
      byte num2 = reader.ReadByte();
      if (num1 != 0)
        server.Passtrough(reader, session);
      switch ((PacketType) num2)
      {
        case PacketType.IDENTITY:
          Ext.HandleIdentity(server, session, reader);
          break;
        case PacketType.CLIENT_VOTE_REQUEST:
          byte index = reader.ReadByte();
          if ((int) index >= this._votes.Length || this._votePeers[session.ID])
            break;
          lock (this._votePeers)
          {
            this._votePeers[session.ID] = true;
            if (this._votePeers.Count<KeyValuePair<ushort, bool>>((Func<KeyValuePair<ushort, bool>, bool>) (e => !e.Value)) <= 0)
            {
              if (this._timerSec > 3)
              {
                this._timer = 1;
                this._timerSec = 4;
              }
            }
          }
          ++this._votes[(int) index].Votes;
          TcpPacket packet = new TcpPacket(PacketType.SERVER_VOTE_SET, new object[3]
          {
            (object) (byte) this._votes[0].Votes,
            (object) (byte) this._votes[1].Votes,
            (object) (byte) this._votes[2].Votes
          });
          server.TCPMulticast(packet);
          break;
      }
    }

    public override void PeerUDPMessage(Server server, IPEndPoint IPEndPoint, ref byte[] data)
    {
    }

    public override void Tick(Server server)
    {
      if (this._timer-- > 0)
        return;
      this._timer = 60;
      --this._timerSec;
      TcpPacket packet = new TcpPacket(PacketType.SERVER_VOTE_TIME_SYNC, new object[1]
      {
        (object) (byte) this._timerSec
      });
      server.TCPMulticast(packet);
      if (this._timerSec > 0)
        return;
      this.CheckVotes(server);
    }

    private void CheckVotes(Server server)
    {
      int max = ((IEnumerable<MapVoteMap>) this._votes).Max<MapVoteMap>((Func<MapVoteMap, int>) (e => e.Votes));
      MapVoteMap[] array = ((IEnumerable<MapVoteMap>) this._votes).Where<MapVoteMap>((Func<MapVoteMap, bool>) (e => e.Votes == max)).ToArray<MapVoteMap>();
      MapVoteMap mapVoteMap = array[this._rand.Next(0, array.Length)];
      server.LastMap = (int) mapVoteMap.MapID;
      server.SetState<CharacterSelect>(new CharacterSelect(mapVoteMap.Map));
    }
  }
}
