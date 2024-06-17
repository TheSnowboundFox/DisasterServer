// Decompiled with JetBrains decompiler
// Type: DisasterServer.FastBitReader
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using System;

#nullable enable
namespace DisasterServer
{
  public class FastBitReader
  {
    public int Position { get; set; }

    public byte ReadByte(ref byte[] data)
    {
      if (this.Position >= data.Length)
        throw new ArgumentOutOfRangeException(nameof (data));
      return data[this.Position++];
    }

    public bool ReadBoolean(ref byte[] data)
    {
      if (this.Position >= data.Length)
        throw new ArgumentOutOfRangeException(nameof (data));
      return Convert.ToBoolean(data[this.Position++]);
    }

    public char ReadChar(ref byte[] data)
    {
      if (this.Position >= data.Length)
        throw new ArgumentOutOfRangeException(nameof (data));
      return (char) data[this.Position++];
    }

    public short ReadShort(ref byte[] data)
    {
      if (this.Position >= data.Length)
        throw new ArgumentOutOfRangeException(nameof (data));
      int num = (int) (short) ((int) data[this.Position] | (int) data[this.Position + 1] << 8);
      this.Position += 2;
      return (short) num;
    }

    public ushort ReadUShort(ref byte[] data)
    {
      if (this.Position >= data.Length)
        throw new ArgumentOutOfRangeException(nameof (data));
      int num = (int) (ushort) ((uint) data[this.Position] | (uint) data[this.Position + 1] << 8);
      this.Position += 2;
      return (ushort) num;
    }

    public int ReadInt(ref byte[] data)
    {
      if (this.Position >= data.Length)
        throw new ArgumentOutOfRangeException(nameof (data));
      int num = (int) data[this.Position] | (int) data[this.Position + 1] << 8 | (int) data[this.Position + 2] << 16 | (int) data[this.Position + 3] << 24;
      this.Position += 4;
      return num;
    }

    public uint ReadUInt(ref byte[] data)
    {
      if (this.Position >= data.Length)
        throw new ArgumentOutOfRangeException(nameof (data));
      int num = (int) data[this.Position] | (int) data[this.Position + 1] << 8 | (int) data[this.Position + 2] << 16 | (int) data[this.Position + 3] << 24;
      this.Position += 4;
      return (uint) num;
    }

    public unsafe float ReadFloat(ref byte[] data)
    {
      if (this.Position >= data.Length)
        throw new ArgumentOutOfRangeException(nameof (data));
      double num;
      fixed (byte* numPtr = &data[this.Position])
        num = (double) *(float*) numPtr;
      this.Position += 4;
      return (float) num;
    }

    public long ReadLong(ref byte[] data)
    {
      if (this.Position >= data.Length)
        throw new ArgumentOutOfRangeException(nameof (data));
      long num = (long) data[this.Position] | (long) data[this.Position + 1] << 8 | (long) data[this.Position + 2] << 16 | (long) data[this.Position + 3] << 24 | (long) data[this.Position + 4] << 32 | (long) data[this.Position + 5] << 40 | (long) data[this.Position + 6] << 48 | (long) data[this.Position + 7] << 56;
      this.Position += 8;
      return num;
    }

    public ulong ReadULong(ref byte[] data)
    {
      if (this.Position >= data.Length)
        throw new ArgumentOutOfRangeException(nameof (data));
      long num = (long) data[this.Position] | (long) data[this.Position + 1] << 8 | (long) data[this.Position + 2] << 16 | (long) data[this.Position + 3] << 24 | (long) data[this.Position + 4] << 32 | (long) data[this.Position + 5] << 40 | (long) data[this.Position + 6] << 48 | (long) data[this.Position + 7] << 56;
      this.Position += 8;
      return (ulong) num;
    }
  }
}
