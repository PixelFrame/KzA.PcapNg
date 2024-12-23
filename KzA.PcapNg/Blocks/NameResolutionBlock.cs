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
    public class NameResolutionBlock : IBlock
    {
        public uint Type => 0x00000004;
        public uint TotalLength => (uint)(32 + Options.Sum(o => o.Size) + Records.Sum(r => r.Size));
        public NameResolutionRecord[] Records { get; set; } = [];
        private uint nrb_record_end => 0;
        public List<OptionBase> Options
        {
            get
            {
                var opts = new List<OptionBase>();
                Options.Add(DnsName);
                if (DnsIP4Addr != null) opts.Add(DnsIP4Addr);
                if (DnsIP6Addr != null) opts.Add(DnsIP6Addr);
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

            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], opt_endofopt);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[(offset + 4)..], TotalLength2);
            return bin;
        }

        public ns_dnsname DnsName { get; set; } = ".";
        public ns_dnsIP4addr? DnsIP4Addr { get; set; }
        public ns_dnsIP6addr? DnsIP6Addr { get; set; }
        public IEnumerable<opt_comment> Comments { get; set; } = [];
    }
}
