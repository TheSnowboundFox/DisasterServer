using System;
using System.Collections.Generic;
using System.Text;

namespace DisasterServer
{
    public class UdpPacket
    {
        private byte[] _buffer = new byte[128];
        private int _position;

        public int Length { get; private set; }

        public UdpPacket(PacketType type, params object[] args)
        {
            Write((byte)0);
            Write((byte)type);
            foreach (object obj in args)
            {
                WriteDynamic(obj);
            }
        }

        public UdpPacket(PacketType type)
        {
            Write((byte)0);
            Write((byte)type);
        }

        private void WriteDynamic(object value)
        {
            switch (value)
            {
                case byte v: Write(v); break;
                case char v: Write(v); break;
                case sbyte v: Write(v); break;
                case bool v: Write(v); break;
                case ushort v: Write(v); break;
                case short v: Write(v); break;
                case uint v: Write(v); break;
                case int v: Write(v); break;
                case float v: Write(v); break;
                case ulong v: Write(v); break;
                case long v: Write(v); break;
                case double v: Write(v); break;
                case string v: Write(v); break;
                default: throw new ArgumentException("Unsupported data type");
            }
        }

        public void Write(byte value)
        {
            lock (_buffer)
            {
                EnsureCapacity(_position + 1);
                _buffer[_position++] = value;
                Length = _position;
            }
        }

        public void Write(char value) => Write((byte)value);

        public void Write(sbyte value) => Write((byte)value);

        public void Write(bool value) => Write(value ? (byte)1 : (byte)0);

        public unsafe void Write(ushort value)
        {
            EnsureCapacity(_position + 2);
            if (BitConverter.IsLittleEndian)
            {
                _buffer[_position++] = (byte)value;
                _buffer[_position++] = (byte)(value >> 8);
            }
            else
            {
                _buffer[_position++] = (byte)(value >> 8);
                _buffer[_position++] = (byte)value;
            }
            Length = _position;
        }

        public unsafe void Write(short value)
        {
            EnsureCapacity(_position + 2);
            if (BitConverter.IsLittleEndian)
            {
                _buffer[_position++] = (byte)value;
                _buffer[_position++] = (byte)(value >> 8);
            }
            else
            {
                _buffer[_position++] = (byte)(value >> 8);
                _buffer[_position++] = (byte)value;
            }
            Length = _position;
        }

        public unsafe void Write(uint value)
        {
            EnsureCapacity(_position + 4);
            if (BitConverter.IsLittleEndian)
            {
                _buffer[_position++] = (byte)value;
                _buffer[_position++] = (byte)(value >> 8);
                _buffer[_position++] = (byte)(value >> 16);
                _buffer[_position++] = (byte)(value >> 24);
            }
            else
            {
                _buffer[_position++] = (byte)(value >> 24);
                _buffer[_position++] = (byte)(value >> 16);
                _buffer[_position++] = (byte)(value >> 8);
                _buffer[_position++] = (byte)value;
            }
            Length = _position;
        }

        public unsafe void Write(int value)
        {
            EnsureCapacity(_position + 4);
            if (BitConverter.IsLittleEndian)
            {
                _buffer[_position++] = (byte)value;
                _buffer[_position++] = (byte)(value >> 8);
                _buffer[_position++] = (byte)(value >> 16);
                _buffer[_position++] = (byte)(value >> 24);
            }
            else
            {
                _buffer[_position++] = (byte)(value >> 24);
                _buffer[_position++] = (byte)(value >> 16);
                _buffer[_position++] = (byte)(value >> 8);
                _buffer[_position++] = (byte)value;
            }
            Length = _position;
        }

        public unsafe void Write(float value)
        {
            uint intValue = *(uint*)&value;
            Write(intValue);
        }

        public unsafe void Write(ulong value)
        {
            EnsureCapacity(_position + 8);
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 8; i++)
                    _buffer[_position++] = (byte)(value >> (i * 8));
            }
            else
            {
                for (int i = 7; i >= 0; i--)
                    _buffer[_position++] = (byte)(value >> (i * 8));
            }
            Length = _position;
        }

        public unsafe void Write(long value)
        {
            EnsureCapacity(_position + 8);
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 8; i++)
                    _buffer[_position++] = (byte)(value >> (i * 8));
            }
            else
            {
                for (int i = 7; i >= 0; i--)
                    _buffer[_position++] = (byte)(value >> (i * 8));
            }
            Length = _position;
        }

        public unsafe void Write(double value)
        {
            ulong intValue = *(ulong*)&value;
            Write(intValue);
        }

        public void Write(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            EnsureCapacity(_position + bytes.Length + 1);
            foreach (byte b in bytes)
                _buffer[_position++] = b;
            _buffer[_position++] = 0;
            Length = _position;
        }

        private void EnsureCapacity(int size)
        {
            if (size > _buffer.Length)
            {
                Array.Resize(ref _buffer, Math.Max(size, _buffer.Length * 2));
            }
        }

        public byte[] ToArray() => _buffer[..Length];
    }
}
