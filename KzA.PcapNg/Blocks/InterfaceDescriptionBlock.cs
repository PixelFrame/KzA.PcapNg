using KzA.PcapNg.Blocks.Options;
using KzA.PcapNg.DataTypes;
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
    public class InterfaceDescriptionBlock : IBlock
    {
        public uint Type => 0x00000001;
        public uint TotalLength => (uint)(20 + Options.Sum(o => o.Size));
        public LinkType LinkType { get; set; } = LinkType.LINKTYPE_ETHERNET;
        private ushort Reserved => 0;
        public uint SnapLen { get; set; } = 0;
        public List<OptionBase> Options
        {
            get
            {
                var opts = new List<OptionBase>();
                if (Name != null) opts.Add(Name);
                if (Description != null) opts.Add(Description);
                if (IPv4Addrs != null) opts.AddRange(IPv4Addrs);
                if (IPv6Addrs != null) opts.AddRange(IPv6Addrs);
                if (MACAddr != null) opts.Add(MACAddr);
                if (EUIAddr != null) opts.Add(EUIAddr);
                if (Speed != null) opts.Add(Speed);
                if (TsResol != null) opts.Add(TsResol);
#pragma warning disable CS0618 // Type or member is obsolete
                if (TZone != null) opts.Add(TZone);
#pragma warning restore CS0618 // Type or member is obsolete
                if (Filter != null) opts.Add(Filter);
                if (OS != null) opts.Add(OS);
                if (FcsLen != null) opts.Add(FcsLen);
                if (TsOffset != null) opts.Add(TsOffset);
                if (Hardware != null) opts.Add(Hardware);
                if (TxSpeed != null) opts.Add(TxSpeed);
                if (RxSpeed != null) opts.Add(RxSpeed);
                if (IANATzName != null) opts.Add(IANATzName);
                if (Comments != null) opts.AddRange(Comments);
                if (CustomOptions != null) opts.AddRange(CustomOptions);
                if (opts.Count > 0) opts.Add(new opt_endofopt());
                return opts;
            }
        }
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

            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], TotalLength2);
            return bin;
        }

        public void Parse(ReadOnlySpan<byte> data, uint totalLen, bool endian)
        {
            LinkType = (LinkType)(endian ? BinaryPrimitives.ReadUInt16LittleEndian(data[8..]) : BinaryPrimitives.ReadUInt16BigEndian(data[8..]));
            SnapLen = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[12..]) : BinaryPrimitives.ReadUInt32BigEndian(data[12..]);
            var offset = 16;
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
                        Name = Encoding.UTF8.GetString(data[(offset + 4)..(offset + length + 4)]);
                        break;
                    case 0x0003:
                        Description = Encoding.UTF8.GetString(data[(offset + 4)..(offset + length + 4)]);
                        break;
                    case 0x0004:
                        IPv4Addrs ??= [];
                        IPv4Addrs.Add((BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 4)..]), BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 8)..])));
                        break;
                    case 0x0005:
                        IPv6Addrs ??= [];
                        IPv6Addrs.Add((BinaryPrimitives.ReadUInt128BigEndian(data[(offset + 4)..]), data[offset + 20]));
                        break;
                    case 0x0006:
                        MACAddr = data[(offset + 4)..(offset + 10)].ToArray();
                        break;
                    case 0x0007:
                        EUIAddr = BinaryPrimitives.ReadUInt64BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x0008:
                        Speed = endian ? BinaryPrimitives.ReadUInt64LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt64BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x0009:
                        TsResol = data[offset + 4];
                        break;
                    case 0x000A:
#pragma warning disable CS0618 // Type or member is obsolete
                        TZone = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 4)..]);
#pragma warning restore CS0618 // Type or member is obsolete
                        break;
                    case 0x000B:
                        Filter = (data[offset + 4], Encoding.UTF8.GetString(data[(offset + 5)..(offset + 4 + length)]));
                        break;
                    case 0x000C:
                        OS = Encoding.UTF8.GetString(data[(offset + 4)..(offset + 4 + length)]);
                        break;
                    case 0x000D:
                        FcsLen = data[offset + 4];
                        break;
                    case 0x000E:
                        TsOffset = endian ? BinaryPrimitives.ReadInt64LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadInt64BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x000F:
                        Hardware = Encoding.UTF8.GetString(data[(offset + 4)..(offset + 4 + length)]);
                        break;
                    case 0x0010:
                        TxSpeed = endian ? BinaryPrimitives.ReadUInt64LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt64BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x0011:
                        RxSpeed = endian ? BinaryPrimitives.ReadUInt64LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt64BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x0012:
                        IANATzName = Encoding.UTF8.GetString(data[(offset + 4)..(offset + 4 + length)]);
                        break;
                    case 0x0001:
                        Comments ??= [];
                        Comments.Add(Encoding.UTF8.GetString(data[(offset + 4)..(offset + 4 + length)]));
                        break;
                    default:
                        var customOption = new CustomOption();
                        customOption.Parse(data[offset..], code, length);
                        CustomOptions ??= [];
                        CustomOptions.Add(customOption);
                        break;
                }
                offset += Misc.DwordPaddedLength(length) + 4;
            }
        }

        public if_name? Name { get; set; }
        public if_description? Description { get; set; }
        public List<if_IPv4addr>? IPv4Addrs { get; set; }
        public List<if_IPv6addr>? IPv6Addrs { get; set; }
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
        public List<opt_comment>? Comments { get; set; }
        public List<CustomOption>? CustomOptions { get; set; }
    }
}
