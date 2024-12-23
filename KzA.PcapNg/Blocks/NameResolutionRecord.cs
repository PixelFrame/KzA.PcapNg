using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks
{
    public struct NameResolutionRecord
    {
        public ushort RecordType = 0;
        public ushort RecordValueLength = 0;
        public uint[] RecordValue = [];

        public NameResolutionRecord()
        {
        }

        public readonly int Size => 4 * (RecordValue.Length + 1);

        public int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, RecordType);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], RecordValueLength);
            var valSpan = new Span<uint>(RecordValue);
            var valBinSpan = MemoryMarshal.AsBytes(valSpan);
            valBinSpan.CopyTo(binSpan[4..]);
            return Size;
        }
    }
}
