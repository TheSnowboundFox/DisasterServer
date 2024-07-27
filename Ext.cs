using DisasterServer;
using DisasterServer.Data;
using DisasterServer.Session;
using ExeNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

#nullable enable
namespace DisasterServer
{
  public static class Ext
  {
    public const int FRAMESPSEC = 60;
    private static Random _rand = new Random();

    public static string ReadStringNull(this BinaryReader reader)
    {
      List<byte> byteList = new List<byte>();
      byte num;
      while (reader.BaseStream.Position < reader.BaseStream.Length && (num = reader.ReadByte()) != (byte) 0)
        byteList.Add(num);
      return Encoding.UTF8.GetString(byteList.ToArray());
    }

    public static T? CreateOfType<T>() => (T) Activator.CreateInstance(typeof (T));

    public static T? CreateOfType<T>(Type value) => (T) Activator.CreateInstance(value);

    public static double Dist(double x, double y, double x2, double y2)
    {
      return Math.Sqrt(Math.Pow(x2 - x, 2.0) + Math.Pow(y2 - y, 2.0));
    }

    public static string ValidateNick(string nick)
    {
      string str = nick;
      char[] chArray = new char[9]
      {
        '\\',
        '/',
        '@',
        '|',
        '№',
        '`',
        '~',
        '&',
        ' '
      };
      foreach (char ch in chArray)
        str = str.Replace(ch.ToString(), "");
      if (str.Length > 0 && !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str))
        return nick;
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
      interpolatedStringHandler.AppendLiteral("/player~ \\");
      interpolatedStringHandler.AppendFormatted<int>(Ext._rand.Next(9999));
      return interpolatedStringHandler.ToStringAndClear();
    }

    public static void HandleIdentity(Server server, TcpSession session, BinaryReader reader)
    {
      ushort num1 = reader.ReadUInt16();
      string nick = reader.ReadStringNull();
      byte num2 = reader.ReadByte();
      sbyte num3 = reader.ReadSByte();
      string str = reader.ReadStringNull();
      if (num1 != (ushort) 100)
      {
        Server server1 = server;
        TcpSession session1 = session;
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 2);
        interpolatedStringHandler.AppendLiteral("Wrong game version (");
        interpolatedStringHandler.AppendFormatted<int>(100);
        interpolatedStringHandler.AppendLiteral(" required, but got ");
        interpolatedStringHandler.AppendFormatted<ushort>(num1);
        interpolatedStringHandler.AppendLiteral(")");
        string stringAndClear = interpolatedStringHandler.ToStringAndClear();
        server1.DisconnectWithReason(session1, stringAndClear);
      }
      else
      {
        if (server.Peers.Count >= 14)
          return;
        DefaultInterpolatedStringHandler interpolatedStringHandler;
        lock (server.Peers)
        {
          if (server.Peers.ContainsKey(session.ID))
          {
            server.Peers[session.ID].Pending = false;
            server.Peers[session.ID].Waiting = server.State.AsState() != 0;
            server.Peers[session.ID].Nickname = Ext.ValidateNick(nick);
            server.Peers[session.ID].Icon = num2;
            server.Peers[session.ID].Pet = num3;
            server.Peers[session.ID].Unique = str;
            if (server.State.AsState() == Session.State.LOBBY)
            {
              interpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 2);
              interpolatedStringHandler.AppendFormatted(nick);
              interpolatedStringHandler.AppendLiteral(" (ID ");
              interpolatedStringHandler.AppendFormatted<ushort>(server.Peers[session.ID].ID);
              interpolatedStringHandler.AppendLiteral(") joined.");
              Terminal.Log(interpolatedStringHandler.ToStringAndClear());
              TcpPacket packet = new TcpPacket(PacketType.SERVER_PLAYER_INFO);
              packet.Write(server.Peers[session.ID].ID);
              packet.Write(nick);
              packet.Write(num2);
              packet.Write(num3);
              server.TCPMulticast(packet, new ushort?(session.ID));
            }
          }
        }
        interpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
        interpolatedStringHandler.AppendLiteral("Indentity recived from ");
        interpolatedStringHandler.AppendFormatted<IPEndPoint>((IPEndPoint) session.RemoteEndPoint);
        interpolatedStringHandler.AppendLiteral(":");
        Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
        interpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
        Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
        Terminal.LogDebug("  UNIQUE: " + str);
        if (KickList.Check(str))
        {
            server.DisconnectWithReason(session, "Kick by host.");
        }
        else
        {
            TcpPacket packet = new TcpPacket(PacketType.SERVER_IDENTITY_RESPONSE);
            packet.Write(server.State.AsState() == Session.State.LOBBY);
            packet.Write(session.ID);
            server.TCPSend(session, packet);
        }
      }
    }
  }
}
