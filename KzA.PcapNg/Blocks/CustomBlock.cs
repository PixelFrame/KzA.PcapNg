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
        public uint TotalLength => (uint)(4 * (5 + CustomData.Length) + Options.Sum(o => o.Size));
        public uint PrivateEnterpriseNumber { get; set; } = 0;
        public uint[] CustomData { get; set; } = [];
        public List<OptionBase> Options { get; set; } = [];
        private uint opt_endofopt => 0;
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
            foreach (var option in Options)
            {
                offset += option.WriteBytes(binSpan[offset..]);
            }

            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], opt_endofopt);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[(offset + 4)..], TotalLength2);
            return bin;
        }

        public void Parse(ReadOnlySpan<byte> data, uint totalLen, bool endian)
        {
            throw new NotImplementedException();
        }
    }
}
