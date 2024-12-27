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
    public class NameResolutionBlock : IBlock
    {
        public uint Type => 0x00000004;
        public uint TotalLength => (uint)(16 + Options.Sum(o => o.Size) + Records.Sum(r => r.Size));
        public List<NameResolutionRecord> Records { get; } = [];
        private uint nrb_record_end => 0;
        public List<OptionBase> Options
        {
            get
            {
                var opts = new List<OptionBase>();
                if (DnsName != null) Options.Add(DnsName);
                if (DnsIP4Addr != null) opts.Add(DnsIP4Addr);
                if (DnsIP6Addr != null) opts.Add(DnsIP6Addr);
                if (Comments != null) opts.AddRange(Comments);
                if (CustomOptions != null) opts.AddRange(CustomOptions);
                if (opts.Count > 0) opts.Add(Misc.opt_endofopt);
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

            int offset = 8;
            foreach (var record in Records)
            {
                offset += record.WriteBytes(binSpan[offset..]);
            }

            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], nrb_record_end);
            offset += 4;

            foreach (var option in Options)
            {
                offset += option.WriteBytes(binSpan[offset..]);
            }

            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], TotalLength2);
            return bin;
        }

        public void Parse(ReadOnlySpan<byte> data, uint totalLen, bool endian)
        {
            var offset = 8;
            while (offset < totalLen - 4)
            {
                var record = new NameResolutionRecord();
                record.RecordType = (DataTypes.NrbRecordType)(endian ? BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]) : BinaryPrimitives.ReadUInt16BigEndian(data[offset..]));
                offset += 2;
                record.RecordValueLength = endian ? BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]) : BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
                offset += 2;
                if (record.RecordType == 0) break;
                record.RecordValue = new uint[Misc.DwordPaddedDwLength(record.RecordValueLength)];
                var valueSpan = new Span<uint>(record.RecordValue);
                var valueBinSpan = MemoryMarshal.AsBytes(valueSpan);
                data[offset..(offset + record.RecordValueLength)].CopyTo(valueBinSpan);
                offset += Misc.DwordPaddedLength(record.RecordValueLength);
                Records.Add(record);
            }
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
                        DnsName = Encoding.UTF8.GetString(data[(offset + 4)..(offset + 4 + length)]);
                        break;
                    case 0x0003:
                        DnsIP4Addr = BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 4)..(offset + 8)]);
                        break;
                    case 0x0004:
                        DnsIP6Addr = BinaryPrimitives.ReadUInt128BigEndian(data[(offset + 4)..(offset + 20)]);
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

        public ns_dnsname? DnsName { get; set; }
        public ns_dnsIP4addr? DnsIP4Addr { get; set; }
        public ns_dnsIP6addr? DnsIP6Addr { get; set; }
        public List<opt_comment>? Comments { get; set; }
        public List<CustomOption>? CustomOptions { get; set; }
    }
}
