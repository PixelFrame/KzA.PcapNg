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
    public class DecryptionSecretsBlock : IBlock
    {
        public uint Type => 0x00000006;
        public uint TotalLength => (uint)(4 * (6 + SecretsData.Length) + Options.Sum(o => o.Size));
        public uint SecretsType { get; set; } = 0;
        public uint SecretsLength { get; set; } = 0;
        public uint[] SecretsData { get; set; } = [];
        public List<OptionBase> Options
        {
            get
            {
                return Comments.Cast<OptionBase>().ToList();
            }
        }
        private uint opt_endofopt => 0;
        public uint TotalLength2 => TotalLength;

        public DecryptionSecretsBlock()
        {
        }

        public byte[] GetBytes()
        {
            var bin = new byte[TotalLength];
            var binSpan = new Span<byte>(bin);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan, Type);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[4..], TotalLength);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[8..], SecretsType);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[12..], SecretsLength);
            var sdataSpan = new Span<uint>(SecretsData);
            var sdataBinSpan = MemoryMarshal.AsBytes(sdataSpan);
            sdataBinSpan.CopyTo(binSpan[16..]);

            int offset = 16 + 4 * SecretsData.Length;
            foreach (var option in Options)
            {
                offset += option.WriteBytes(binSpan[offset..]);
            }

            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], opt_endofopt);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[(offset + 4)..], TotalLength2);
            return bin;
        }

        public IEnumerable<opt_comment> Comments { get; set; } = [];
    }
}
