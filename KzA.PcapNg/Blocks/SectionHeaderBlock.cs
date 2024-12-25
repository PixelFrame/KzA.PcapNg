using KzA.PcapNg.Blocks.Options;
using KzA.PcapNg.Helper;
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
        public bool LittleEndian { get; private set; } = true;
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
                options.AddRange(Comments);
                options.AddRange(CustomOptions);
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

        public void Parse(ReadOnlySpan<byte> data, uint totalLen, bool endian)
        {
            SectionLength = endian ? BinaryPrimitives.ReadInt64LittleEndian(data[16..]) : BinaryPrimitives.ReadInt64BigEndian(data[16..]);
            var offset = 24;
            var reachedEnd = false;
            while (offset < totalLen - 4 && !reachedEnd)
            {
                var code = endian ? BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]) : BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
                var length = endian ? BinaryPrimitives.ReadUInt16LittleEndian(data[(offset + 2)..]) : BinaryPrimitives.ReadUInt16BigEndian(data[(offset + 2)..]);
                switch (code)
                {
                    case 0x0000:
                        reachedEnd = true;
                        break;
                    case 0x0002:
                        Hardware = Encoding.UTF8.GetString(data[(offset + 4)..(offset + 4 + length)]);
                        break;
                    case 0x0003:
                        OS = Encoding.UTF8.GetString(data[(offset + 4)..(offset + 4 + length)]);
                        break;
                    case 0x0004:
                        UserAppl = Encoding.UTF8.GetString(data[(offset + 4)..(offset + 4 + length)]);
                        break;
                    case 0x0001:
                        Comments.Add(Encoding.UTF8.GetString(data[(offset + 4)..(offset + 4 + length)]));
                        break;
                    default:
                        var customOption = new CustomOption();
                        customOption.Parse(data[offset..], code, length);
                        CustomOptions.Add(customOption);
                        break;
                }
                offset += Misc.DwordPaddedLength(length) + 4;
            }
        }

        public void Parse(BinaryReader binReader)
        {
            binReader.BaseStream.Seek(4, SeekOrigin.Current);
            uint len = binReader.ReadUInt32();
            uint bom = binReader.ReadUInt32();
            LittleEndian = bom == 0x1A2B3C4D;
            if(!LittleEndian) len = BinaryPrimitives.ReverseEndianness(len);
            binReader.BaseStream.Seek(-12, SeekOrigin.Current);
            var data = binReader.ReadBytes((int)len);
            Parse(data, len, LittleEndian);
        }

        public shb_hardware? Hardware { get; set; }
        public shb_os? OS { get; set; }
        public shb_userappl? UserAppl { get; set; }
        public List<opt_comment> Comments { get; set; } = [];
        public List<CustomOption> CustomOptions { get; set; } = [];
    }
}