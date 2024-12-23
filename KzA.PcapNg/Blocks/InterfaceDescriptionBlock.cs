using KzA.PcapNg.Blocks.Options;
using KzA.PcapNg.DataTypes;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks
{
    public class InterfaceDescriptionBlock : IBlock
    {
        public uint Type => 0x00000001;
        public uint TotalLength => (uint)(24 + Options.Sum(o => o.Size));
        public LinkType LinkType { get; set; } = LinkType.LINKTYPE_ETHERNET;
        private ushort Reserved => 0;
        public uint SnapLen { get; set; } = 0;
        public List<OptionBase> Options
        {
            get
            {
                var list = new List<OptionBase>();
                if (Name != null) list.Add(Name);
                if (Description != null) list.Add(Description);
                list.AddRange(IPv4Addrs);
                list.AddRange(IPv6Addrs);
                if (MACAddr != null) list.Add(MACAddr);
                if (EUIAddr != null) list.Add(EUIAddr);
                if (Speed != null) list.Add(Speed);
                if (TsResol != null) list.Add(TsResol);
                if (Filter != null) list.Add(Filter);
                if (OS != null) list.Add(OS);
                if (FcsLen != null) list.Add(FcsLen);
                if (TsOffset != null) list.Add(TsOffset);
                if (Hardware != null) list.Add(Hardware);
                if (TxSpeed != null) list.Add(TxSpeed);
                if (RxSpeed != null) list.Add(RxSpeed);
                if (IANATzName != null) list.Add(IANATzName);
                list.AddRange(Comments);
                return list;
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
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[8..], (ushort)LinkType);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[10..], Reserved);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[12..], SnapLen);

            int offset = 16;
            foreach (var option in Options)
            {
                offset += option.WriteBytes(binSpan[offset..]);
            }

            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], opt_endofopt);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[(offset + 4)..], TotalLength2);
            return bin;
        }

        public if_name? Name { get; set; }
        public if_description? Description { get; set; }
        public IEnumerable<if_IPv4addr> IPv4Addrs { get; set; } = [];
        public IEnumerable<if_IPv6addr> IPv6Addrs { get; set; } = [];
        public if_MACaddr? MACAddr { get; set; }
        public if_EUIaddr? EUIAddr { get; set; }
        public if_speed? Speed { get; set; }
        public if_tsresol? TsResol { get; set; }
        [Obsolete("Use IANATzName instead")]
        public if_tzone? TZone { get; set; }
        public if_filter? Filter { get; set; }
        public if_os? OS { get; set; }
        public if_fcslen? FcsLen { get; set; }
        public if_tsoffset? TsOffset { get; set; }
        public if_hardware? Hardware { get; set; }
        public if_txspeed? TxSpeed { get; set; }
        public if_rxspeed? RxSpeed { get; set; }
        public if_iana_tzname? IANATzName { get; set; }
        public IEnumerable<opt_comment> Comments { get; set; } = [];
    }
}
