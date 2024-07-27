using DisasterServer.Data;
using DisasterServer.Maps;
using DisasterServer.Session;
using ExeNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

#nullable enable
namespace DisasterServer.State
{
  internal class CharacterSelect : DisasterServer.State.State
  {
    private Random _rand = new Random();
    private int _timeout;
    private Peer _exe;
    private Map _map;
    private Dictionary<ushort, int> _lastPackets = new Dictionary<ushort, int>();
    public List<(TcpPacket, ushort)> CharChoices = new List<(TcpPacket, ushort)>();
    public (TcpPacket, ushort) Choice;
    public CharacterSelect(Map map) => this._map = map;

    public override DisasterServer.Session.State AsState() => DisasterServer.Session.State.CHARACTERSELECT;

    public override void PeerJoined(Server server, TcpSession session, Peer peer)
    {
    }

    public override void PeerLeft(Server server, TcpSession session, Peer peer)
    {
      lock (server.Peers)
      {
        if (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) <= 1 || this._exe == peer)
        {
          server.SetState<Lobby>();
          return;
        }
        int num = 0;
        foreach (Peer peer1 in server.Peers.Values)
        {
          if (!peer1.Waiting && peer1.Player.Character != Character.None && (peer1.Player.Character != Character.Exe || peer1.Player.ExeCharacter != ExeCharacter.None))
            ++num;
        }
        if (num >= server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting))){
            foreach((TcpPacket, ushort?) CharChoice in CharChoices){
              server.TCPMulticast(CharChoice.Item1, CharChoice.Item2);
            }
          server.SetState<Game>(new Game(this._map, this._exe.ID));
        }
      }
      lock (this._lastPackets)
        this._lastPackets.Remove(peer.ID);
    }

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      lock (this._lastPackets)
        this._lastPackets[session.ID] = 0;
      int num1 = reader.ReadBoolean() ? 1 : 0;
      byte num2 = reader.ReadByte();
      if (num1 != 0)
        server.Passtrough(reader, session);
      switch ((PacketType) num2)
      {
        case PacketType.IDENTITY:
          Ext.HandleIdentity(server, session, reader);
          break;
        case PacketType.CLIENT_REQUEST_CHARACTER:
          byte num3 = reader.ReadByte();
          bool flag = true;
          int num4 = 0;
          lock (server.Peers)
          {
            if (server.Peers[session.ID].Player.Character != Character.None)
              break;
            foreach (Peer peer in server.Peers.Values)
            {
              if (!peer.Waiting)
              {
               // if (peer.Player.Character == (Character) num3)
               //   flag = false;
                if (peer.Player.Character != Character.None && (peer.Player.Character != Character.Exe || peer.Player.ExeCharacter != ExeCharacter.None))
                  ++num4;
              }
            }
            if (flag)
            {
              if (!server.Peers.ContainsKey(session.ID))
                break;
              Peer peer = server.Peers[session.ID];
              peer.Player.Character = (Character) num3;
              TcpPacket packet1 = new TcpPacket(PacketType.SERVER_LOBBY_CHARACTER_RESPONSE, new object[2]
              {
                (object) num3,
                (object) true
              });
              server.TCPSend(session, packet1);
              TcpPacket packet2 = new TcpPacket(PacketType.SERVER_LOBBY_CHARACTER_CHANGE);
              packet2.Write(session.ID);
              packet2.Write(num3);
              Choice = (packet2, session.ID);
              CharChoices.Add(Choice);
              //server.TCPMulticast(packet2, new ushort?(session.ID));
              DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 2);
              interpolatedStringHandler.AppendFormatted(peer.Nickname);
              interpolatedStringHandler.AppendLiteral(" chooses ");
              interpolatedStringHandler.AppendFormatted<Character>((Character) num3);
              Terminal.Log(interpolatedStringHandler.ToStringAndClear());
              int num5;
              if ((num5 = num4 + 1) < server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)))
                break;
              foreach((TcpPacket, ushort?) CharChoice in CharChoices){                
                server.TCPMulticast(CharChoice.Item1, CharChoice.Item2);
              }
              server.SetState<Game>(new Game(this._map, this._exe.ID));
              break;
            }
            TcpPacket packet = new TcpPacket(PacketType.SERVER_LOBBY_CHARACTER_RESPONSE, new object[2]
            {
              (object) num3,
              (object) false
            });
            server.TCPSend(session, packet);
            break;
          }
        case PacketType.CLIENT_REQUEST_EXECHARACTER:
          int num6 = (int) reader.ReadByte() - 1;
          lock (server.Peers)
          {
            if (server.Peers[session.ID].Player.Character != Character.Exe || server.Peers[session.ID].Player.ExeCharacter != ExeCharacter.None)
              break;
            int num7 = 0;
            foreach (Peer peer in server.Peers.Values)
            {
              if (!peer.Waiting && peer.Player.Character != Character.None && (peer.Player.Character != Character.Exe || peer.Player.ExeCharacter != ExeCharacter.None))
                ++num7;
            }
            server.Peers[session.ID].Player.ExeCharacter = (ExeCharacter) num6;
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 2);
            interpolatedStringHandler.AppendFormatted(server.Peers[session.ID].Nickname);
            interpolatedStringHandler.AppendLiteral(" chooses ");
            interpolatedStringHandler.AppendFormatted<ExeCharacter>((ExeCharacter) num6);
            Terminal.Log(interpolatedStringHandler.ToStringAndClear());
            TcpPacket packet3 = new TcpPacket(PacketType.SERVER_LOBBY_EXECHARACTER_RESPONSE, new object[1]
            {
              (object) num6
            });
            server.TCPSend(session, packet3);
            TcpPacket packet4 = new TcpPacket(PacketType.SERVER_LOBBY_CHARACTER_CHANGE);
            packet4.Write(session.ID);
            packet4.Write(num6);
            server.TCPMulticast(packet4, new ushort?(session.ID));
            int num8;
            if ((num8 = num7 + 1) < server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)))
              break;
            foreach((TcpPacket, ushort?) CharChoice in CharChoices){
              server.TCPMulticast(CharChoice.Item1, CharChoice.Item2);
            }
            server.SetState<Game>(new Game(this._map, this._exe.ID));
            break;
          }
      }
    }

    public override void PeerUDPMessage(Server server, IPEndPoint IPEndPoint, ref byte[] data)
    {
    }

    public override void Init(Server server)
    {
      lock (server.Peers)
      {
        if (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) <= 1 && !(this._map is FartZone))
          server.SetState<Lobby>();
        int num = 0;
        foreach (Peer peer in server.Peers.Values)
        {
          if (peer.Pending)
            server.DisconnectWithReason((TcpSession) server.GetSession(peer.ID), "PacketType.CLIENT_REQUESTED_INFO missing.");
          else if (!peer.Waiting)
          {
            lock (this._lastPackets)
              this._lastPackets.Add(peer.ID, 0);
            peer.Player = new Player();
            ++num;
          }
        }
        this._exe = this.ChooseExe(server) ?? server.Peers[(ushort) 0];
        this._exe.Player.Character = Character.Exe;
        this._exe.ExeChance = 0;
        foreach (Peer peer in server.Peers.Values)
        {
          if (!peer.Waiting)
          {
            if (peer.Player.Character != Character.Exe)
              peer.ExeChance += this._rand.Next(4, 10);
            else
              peer.ExeChance += this._rand.Next(0, 2);
          }
        }
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
        interpolatedStringHandler.AppendLiteral("Map is ");
        interpolatedStringHandler.AppendFormatted<Map>(this._map);
        Terminal.Log(interpolatedStringHandler.ToStringAndClear());
        TcpPacket packet = new TcpPacket(PacketType.SERVER_LOBBY_EXE);
        packet.Write(this._exe.ID);
        packet.Write((ushort) Array.IndexOf<Type>(Lobby.Maps, this._map?.GetType()));
        server.TCPMulticast(packet);
      }
    }

    private Peer? ChooseExe(Server server)
    {
      Dictionary<ushort, double> dictionary = new Dictionary<ushort, double>();
      double num1 = 0.0;
      lock (server.Peers)
      {
        foreach (Peer peer in server.Peers.Values)
        {
          if (!peer.Waiting)
          {
            num1 += (double) peer.ExeChance;
            int exeChance = peer.ExeChance;
            dictionary.Add(peer.ID, num1);
          }
        }
        double num2 = this._rand.NextDouble() * num1;
        foreach (KeyValuePair<ushort, double> keyValuePair in dictionary)
        {
          KeyValuePair<ushort, double> chance = keyValuePair;
          if (chance.Value >= num2)
            return server.Peers.Values.FirstOrDefault<Peer>((Func<Peer, bool>) (e => (int) e.ID == (int) chance.Key && !e.Waiting));
        }
        return server.Peers.Values.FirstOrDefault<Peer>();
      }
    }

    public override void Tick(Server server) => this.DoTimeout(server);

    private void DoTimeout(Server server)
    {
      if (this._timeout-- > 0)
        return;
      lock (server.Peers)
      {
        foreach (Peer peer1 in server.Peers.Values)
        {
          Peer peer = peer1;
          if (!peer.Waiting && this._lastPackets.Any<KeyValuePair<ushort, int>>((Func<KeyValuePair<ushort, int>, bool>) (e => (int) e.Key == (int) peer.ID)))
          {
            lock (this._lastPackets)
            {
              if (peer.Player.Character != Character.None && peer.Player.Character != Character.Exe)
                this._lastPackets[peer.ID] = 0;
              else if (peer.Player.Character == Character.Exe && peer.Player.ExeCharacter != ExeCharacter.None)
                this._lastPackets[peer.ID] = 0;
              else if (this._lastPackets[peer.ID] >= 1800)
              {
                server.DisconnectWithReason((TcpSession) server.GetSession(peer.ID), "AFK or Timeout");
              }
              else
              {
                server.TCPSend((TcpSession) server.GetSession(peer.ID), new TcpPacket(PacketType.SERVER_CHAR_TIME_SYNC, new object[1]
                {
                  (object) (byte) (30 - this._lastPackets[peer.ID] / 60)
                }));
                this._lastPackets[peer.ID] += 60;
              }
            }
          }
        }
      }
      this._timeout = 60;
    }
  }
}
