using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks.Options
{
    public class ns_dnsname : Utf8StringOption
    {
        public override ushort Code { get => 2; set => throw new InvalidOperationException(); }
        public ns_dnsname() { }
        public ns_dnsname(string v) { _str = v; }
        public static implicit operator ns_dnsname(string v) => new(v);
    }

    public class ns_dnsIP4addr : OptionBase
    {
        public uint Address { get; set; }

        public override ushort Code { get => 3; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 4; set => throw new InvalidOperationException(); }
        public override uint[] Value { get => [BinaryPrimitives.ReverseEndianness(Address)]; set => throw new InvalidOperationException(); }

        public override int Size => 8;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt32BigEndian(binSpan[4..], Address);
            return 8;
        }

        public ns_dnsIP4addr() { }
        public ns_dnsIP4addr(uint v) { Address = v; }
        public static implicit operator ns_dnsIP4addr(uint v) => new(v);
    }

    public class ns_dnsIP6addr : OptionBase
    {
        public UInt128 Address { get; set; }

        public override ushort Code { get => 4; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 16; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                var result = new uint[4];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                BinaryPrimitives.WriteUInt128BigEndian(resultSpan, Address);
                return result;
            }
            set => throw new InvalidOperationException();
        }

        public override int Size => 20;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt128BigEndian(binSpan[4..], Address);
            return 20;
        }

        public ns_dnsIP6addr() { }
        public ns_dnsIP6addr(UInt128 v) { Address = v; }
        public static implicit operator ns_dnsIP6addr(UInt128 v) => new(v);
    }
}
