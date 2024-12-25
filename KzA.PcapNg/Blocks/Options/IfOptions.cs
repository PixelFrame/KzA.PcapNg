using KzA.PcapNg.Helper;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks.Options
{
    public class if_name : Utf8StringOption
    {
        public override ushort Code { get => 2; set => throw new InvalidOperationException(); }
        public if_name() { }
        public if_name(string v) { _str = v; }
        public static implicit operator if_name(string v) => new(v);
    }

    public class if_description : Utf8StringOption
    {
        public override ushort Code { get => 3; set => throw new InvalidOperationException(); }
        public if_description() { }
        public if_description(string v) { _str = v; }
        public static implicit operator if_description(string v) => new(v);
    }

    public class if_IPv4addr : OptionBase
    {
        public uint Address { get; set; }
        public uint Mask { get; set; }

        public override ushort Code { get => 4; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 8; set => throw new InvalidOperationException(); }
        public override uint[] Value { get => [BinaryPrimitives.ReverseEndianness(Address), BinaryPrimitives.ReverseEndianness(Mask)]; set => throw new InvalidOperationException(); }

        public override int Size => 12;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt32BigEndian(binSpan[4..], Address);   //Address and Mask are treated as 4 octets
            BinaryPrimitives.WriteUInt32BigEndian(binSpan[8..], Mask);
            return 12;
        }
        public if_IPv4addr() { }
        public if_IPv4addr(uint address, uint mask) { Address = address; Mask = mask; }
        public static implicit operator if_IPv4addr((uint address, uint mask) v) => new(v.address, v.mask);
    }

    public class if_IPv6addr : OptionBase
    {
        public UInt128 Address { get; set; }
        public byte PrefixLen { get; set; }

        public override ushort Code { get => 5; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 17; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                var result = new uint[5];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                BinaryPrimitives.WriteUInt128BigEndian(resultSpan, Address);
                resultSpan[16] = PrefixLen;
                return result;
            }
            set => throw new InvalidOperationException();
        }

        public override int Size => 24;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt128BigEndian(binSpan[4..], Address);
            binSpan[20] = PrefixLen;
            return 24;
        }
        public if_IPv6addr() { }
        public if_IPv6addr(UInt128 address, byte prefixLen) { Address = address; PrefixLen = prefixLen; }
        public static implicit operator if_IPv6addr((UInt128 address, byte prefixLen) v) => new(v.address, v.prefixLen);
    }

    public class if_MACaddr : OptionBase
    {
        private byte[] _address = new byte[6];
        public byte[] Address => _address;
        public string AddressString
        {
            get => string.Join(':', _address.Select(b => b.ToString("X2")));
            set
            {
                _address = Convert.FromHexString(value.Replace(":", string.Empty).Replace("-", string.Empty));
            }
        }

        public override ushort Code { get => 6; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 6; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                var result = new uint[2];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                new Span<byte>(_address).CopyTo(resultSpan);
                return result;
            }
            set => throw new InvalidOperationException();
        }

        public override int Size => 12;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            new Span<byte>(_address).CopyTo(binSpan[4..]);
            return 12;
        }

        public if_MACaddr() { }
        public if_MACaddr(byte[] address) { address.CopyTo(_address, 0); }
        public if_MACaddr(string address) { AddressString = address; }
        public static implicit operator if_MACaddr(byte[] v) => new(v);
        public static implicit operator if_MACaddr(string v) => new(v);
    }

    public class if_EUIaddr : OptionBase
    {
        public ulong Address { get; set; }

        public override ushort Code { get => 7; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 8; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                var result = new uint[2];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                BinaryPrimitives.WriteUInt64BigEndian(resultSpan, Address);
                return result;
            }
            set => throw new InvalidOperationException();
        }

        public override int Size => 12;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt64BigEndian(binSpan[4..], Address);
            return 12;
        }

        public if_EUIaddr() { }
        public if_EUIaddr(ulong address) { Address = address; }
        public static implicit operator if_EUIaddr(ulong v) => new(v);
    }

    public class if_speed : U64Option
    {
        public override ushort Code { get => 8; set => throw new InvalidOperationException(); }

        public if_speed() { }
        public if_speed(ulong v) { U64Value = v; }
        public static implicit operator if_speed(ulong v) => new(v);
    }

    public class if_tsresol : OptionBase
    {
        public bool BaseBit { get; set; } = false;
        public byte Exponent { get; set; } = 6;

        public override ushort Code { get => 9; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 1; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                if (BaseBit) return [((uint)Exponent) << 24];
                else return [((uint)Exponent | 0x80) << 24];
            }
            set => throw new InvalidOperationException();
        }

        public override int Size => 8;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            binSpan[4] = Exponent;
            if (BaseBit) binSpan[4] |= 0x80;
            return 8;
        }

        public if_tsresol() { }
        public if_tsresol(bool baseBit, byte exponent) { BaseBit = baseBit; Exponent = exponent; }
        public static implicit operator if_tsresol((bool baseBit, byte exponent) v) => new(v.baseBit, v.exponent);
        public static implicit operator if_tsresol(byte v) => new((v & 0x80) != 0, (byte)(v & 0x7F));
    }

    [Obsolete("Use if_iana_tzname instead")]
    public class if_tzone : OptionBase
    {
        public uint Timezone { get; set; }

        public override ushort Code { get => 10; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 4; set => throw new InvalidOperationException(); }
        public override uint[] Value { get => [Timezone]; set => throw new InvalidOperationException(); }

        public override int Size => 8;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[4..], Timezone);
            return 8;
        }

        public if_tzone() { }
        public if_tzone(uint timezone) { Timezone = timezone; }
        public static implicit operator if_tzone(uint v) => new(v);
    }

    public class if_filter : OptionBase
    {
        public byte FilterCode { get; set; } = 0;
        public string Filter { get; set; } = string.Empty;

        public override ushort Code { get => 11; set => throw new InvalidOperationException(); }
        public override ushort Length { get => (ushort)(Encoding.UTF8.GetByteCount(Filter) + 1); set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                var paddedLen = Misc.DwordPaddedDwLength(Length);
                var result = new uint[paddedLen];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                resultSpan[0] = FilterCode;
                Encoding.UTF8.GetBytes(Filter, resultSpan[1..]);
                return result;
            }
            set => throw new InvalidOperationException();
        }

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            binSpan[4] = FilterCode;
            Encoding.UTF8.GetBytes(Filter, binSpan[5..]);
            return Size;
        }

        public if_filter() { }
        public if_filter(byte filterCode, string filter) { FilterCode = filterCode; Filter = filter; }
        public static implicit operator if_filter((byte filterCode, string filter) v) => new(v.filterCode, v.filter);
    }

    public class if_os : Utf8StringOption
    {
        public override ushort Code { get => 12; set => throw new InvalidOperationException(); }

        public if_os() { }
        public if_os(string v) { _str = v; }
        public static implicit operator if_os(string v) => new(v);
    }

    public class if_fcslen : OptionBase
    {
        public byte FcsLen { get; set; } = 0;

        public override ushort Code { get => 13; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 1; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get => [((uint)FcsLen) << 24];
            set => throw new InvalidOperationException();
        }

        public override int Size => 8;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            binSpan[4] = FcsLen;
            return 8;
        }

        public if_fcslen() { }
        public if_fcslen(byte fcsLen) { FcsLen = fcsLen; }
        public static implicit operator if_fcslen(byte v) => new(v);
    }

    public class if_tsoffset : OptionBase
    {
        public long Offset { get; set; }

        public override ushort Code { get => 14; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 8; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                var result = new uint[2];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                BinaryPrimitives.WriteInt64LittleEndian(resultSpan, Offset);
                return result;
            }
            set => throw new InvalidOperationException();
        }

        public override int Size => 12;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteInt64LittleEndian(binSpan[4..], Offset);
            return 12;
        }

        public if_tsoffset() { }
        public if_tsoffset(long offset) { Offset = offset; }
        public static implicit operator if_tsoffset(long v) => new(v);
    }

    public class if_hardware : Utf8StringOption
    {
        public override ushort Code { get => 15; set => throw new InvalidOperationException(); }

        public if_hardware() { }
        public if_hardware(string v) { _str = v; }
        public static implicit operator if_hardware(string v) => new(v);
    }

    public class if_txspeed : U64Option
    {
        public override ushort Code { get => 16; set => throw new InvalidOperationException(); }

        public if_txspeed() { }
        public if_txspeed(ulong v) { U64Value = v; }
        public static implicit operator if_txspeed(ulong v) => new(v);
    }

    public class if_rxspeed : U64Option
    {
        public override ushort Code { get => 17; set => throw new InvalidOperationException(); }

        public if_rxspeed() { }
        public if_rxspeed(ulong v) { U64Value = v; }
        public static implicit operator if_rxspeed(ulong v) => new(v);
    }

    public class if_iana_tzname : Utf8StringOption
    {
        public override ushort Code { get => 18; set => throw new InvalidOperationException(); }

        public if_iana_tzname() { }
        public if_iana_tzname(string v) { _str = v; }
        public static implicit operator if_iana_tzname(string v) => new(v);
    }
}
