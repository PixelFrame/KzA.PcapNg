using KzA.PcapNg.Blocks.Options;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks
{
    public class CustomBlock : IBlock
    {
        public uint Type => 0x00000BAD;
        public uint TotalLength => (uint)(4 * (4 + CustomData.Length) + (Options?.Sum(o => o.Size) ?? 0) + ((Options?.Count > 0) ? 4 : 0));
        public uint PrivateEnterpriseNumber { get; set; } = 0;
        public uint[] CustomData { get; set; } = [];
        public List<OptionBase>? Options { get; set; }
        public uint TotalLength2 => TotalLength;

        public byte[] GetBytes()
        {
            var bin = new byte[TotalLength];
            var binSpan = new Span<byte>(bin);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan, Type);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[4..], TotalLength);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[8..], PrivateEnterpriseNumber);
            var cdataSpan = new Span<uint>(CustomData);
            var cdataBinSpan = MemoryMarshal.AsBytes(cdataSpan);
            cdataBinSpan.CopyTo(binSpan[12..]);

            int offset = 12 + 4 * CustomData.Length;
            if (Options != null)
            {
                foreach (var option in Options)
                {
                    offset += option.WriteBytes(binSpan[offset..]);
                }
            }

            if (Options?.Count > 0)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], 0);
                offset += 4;
            }
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], TotalLength2);
            return bin;
        }

        public void Parse(ReadOnlySpan<byte> data, uint totalLen, bool endian)
        {
            PrivateEnterpriseNumber = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[8..]) : BinaryPrimitives.ReadUInt32BigEndian(data[8..]);
            // Unable to implement further as we cannot tell where is the end of CustomData...
        }
    }
}
