using KzA.PcapNg.Blocks.Options;
using KzA.PcapNg.Helper;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KzA.PcapNg.Blocks
{
    public class InterfaceStatisticsBlock : IBlock
    {
        public uint Type => 0x00000005;
        public uint TotalLength => (uint)(28 + Options.Sum(o => o.Size));
        public uint InterfaceID { get; set; } = 0;
        public uint TimestampUpper { get; set; } = 0;
        public uint TimestampLower { get; set; } = 0;
        public List<OptionBase> Options
        {
            get
            {
                var opts = new List<OptionBase>();
                if (StartTime != null) opts.Add(StartTime);
                if (EndTime != null) opts.Add(EndTime);
                if (IfRecv != null) opts.Add(IfRecv);
                if (IfDrop != null) opts.Add(IfDrop);
                if (FilterAccept != null) opts.Add(FilterAccept);
                if (OSDrop != null) opts.Add(OSDrop);
                if (UsrDeliv != null) opts.Add(UsrDeliv);
                opts.AddRange(Comments);
                opts.AddRange(CustomOptions);
                return opts;
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
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[8..], InterfaceID);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[12..], TimestampUpper);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[16..], TimestampLower);

            int offset = 20;
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
            InterfaceID = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[8..]) : BinaryPrimitives.ReadUInt32BigEndian(data[8..]);
            TimestampUpper = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[12..]) : BinaryPrimitives.ReadUInt32BigEndian(data[12..]);
            TimestampLower = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[16..]) : BinaryPrimitives.ReadUInt32BigEndian(data[16..]);
            var offset = 20;
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
                        StartTime = (endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 4)..]),
                            endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[(offset + 8)..]) : BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 8)..]));
                        break;
                    case 0x0003:
                        EndTime = (endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 4)..]),
                            endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[(offset + 8)..]) : BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 8)..]));
                        break;
                    case 0x0004:
                        IfRecv = endian ? BinaryPrimitives.ReadUInt64LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt64BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x0005:
                        IfDrop = endian ? BinaryPrimitives.ReadUInt64LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt64BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x0006:
                        FilterAccept = endian ? BinaryPrimitives.ReadUInt64LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt64BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x0007:
                        OSDrop = endian ? BinaryPrimitives.ReadUInt64LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt64BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x0008:
                        UsrDeliv = endian ? BinaryPrimitives.ReadUInt64LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt64BigEndian(data[(offset + 4)..]);
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

        public isb_starttime? StartTime { get; set; }
        public isb_endtime? EndTime { get; set; }
        public isb_ifrecv? IfRecv { get; set; }
        public isb_ifdrop? IfDrop { get; set; }
        public isb_filteraccept? FilterAccept { get; set; }
        public isb_osdrop? OSDrop { get; set; }
        public isb_usrdeliv? UsrDeliv { get; set; }
        public List<opt_comment> Comments { get; set; } = [];
        public List<CustomOption> CustomOptions { get; set; } = [];
    }
}
