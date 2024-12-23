using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks.Options
{
    public abstract class U64Option : OptionBase
    {
        public ulong U64Value { get; set; }

        public override ushort Length { get => 8; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                var result = new uint[2];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                BinaryPrimitives.WriteUInt64LittleEndian(resultSpan, U64Value);
                return result;
            }
            set => throw new InvalidOperationException();
        }

        public override int Size => 12;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt64LittleEndian(binSpan[4..], U64Value);
            return 12;
        }
    }
}
