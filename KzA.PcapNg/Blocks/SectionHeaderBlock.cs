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
    public class SectionHeaderBlock : IBlock
    {
        public uint Type => 0x0A0D0D0A;
        public uint TotalLength => (uint)(32 + Options.Sum(o => o.Size));
        public uint BOM => 0x1A2B3C4D;
        public ushort MajorVersion => 1;
        public ushort MinorVersion => 0;
        public long SectionLength { get; internal set; } = -1;
        public List<OptionBase> Options
        {
            get
            {
                var options = new List<OptionBase>();
                if (Hardware != null) options.Add(Hardware);
                if (OS != null) options.Add(OS);
                if (UserAppl != null) options.Add(UserAppl);
                if (Comment != null) options.Add(Comment);
                return options;
            }
        }
        private uint opt_endofopt => 0;
        public uint TotalLength2 => TotalLength;

        public byte[] GetBytes()
        {
            var bin = new byte[TotalLength];
            var binSpan = new Span<byte>(bin);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan, Type);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[4..], TotalLength);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[8..], BOM);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[12..], MajorVersion);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[14..], MinorVersion);
            BinaryPrimitives.WriteInt64LittleEndian(binSpan[16..], SectionLength);

            int offset = 24;
            foreach (var option in Options)
            {
                offset += option.WriteBytes(binSpan[offset..]);
            }

            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], opt_endofopt);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[(offset + 4)..], TotalLength2);
            return bin;
        }

        public shb_hardware? Hardware { get; set; }
        public shb_os? OS { get; set; }
        public shb_userappl? UserAppl { get; set; }
        public opt_comment? Comment { get; set; }
    }
}