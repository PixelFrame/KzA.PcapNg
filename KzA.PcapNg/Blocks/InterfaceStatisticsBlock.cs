using KzA.PcapNg.Blocks.Options;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public isb_starttime? StartTime { get; set; }
        public isb_endtime? EndTime { get; set; }
        public isb_ifrecv? IfRecv { get; set; }
        public isb_ifdrop? IfDrop { get; set; }
        public isb_filteraccept? FilterAccept { get; set; }
        public isb_osdrop? OSDrop { get; set; }
        public isb_usrdeliv? UsrDeliv { get; set; }
        public IEnumerable<opt_comment> Comments { get; set; } = [];
    }
}
