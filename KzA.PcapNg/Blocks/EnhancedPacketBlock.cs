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
    public class EnhancedPacketBlock : IBlock
    {
        public uint Type => 0x00000006;
        public uint TotalLength => (uint)(4 * (9 + PacketData.Length) + Options.Sum(o => o.Size));
        public uint InterfaceID { get; set; } = 0;
        public uint TimestampUpper { get; set; } = 0;
        public uint TimestampLower { get; set; } = 0;
        public uint CapturedPacketLength { get; set; } = 0;
        public uint OriginalPacketLength { get; set; } = 0;
        public uint[] PacketData { get; set; } = [];
        public List<OptionBase> Options
        {
            get
            {
                var opts = new List<OptionBase>
                {
                    FlagsOpt
                };
                opts.AddRange(Hash);
                if (DropCount != null) opts.Add(DropCount);
                if (PacketId != null) opts.Add(PacketId);
                if (Queue != null) opts.Add(Queue);
                opts.AddRange(VerDict);
                if (PidTid != null) opts.Add(PidTid);
                opts.AddRange(Comments);
                return opts;
            }
        }
        public uint opt_endofopt => 0;
        public uint TotalLength2 => TotalLength;

        public void WritePacketData(byte[] data, uint originalLength = 0)
        {
            CapturedPacketLength = (uint)data.Length;
            if (originalLength < CapturedPacketLength) OriginalPacketLength = CapturedPacketLength;
            else OriginalPacketLength = originalLength;
            PacketData = new uint[Misc.DwordPaddedLength(CapturedPacketLength)];
            var dataSpan = new Span<byte>(data);
            var pdataSpan = new Span<uint>(PacketData);
            var pdataBinSpan = MemoryMarshal.AsBytes(pdataSpan);
            dataSpan.CopyTo(pdataBinSpan);
        }

        public byte[] GetBytes()
        {
            var bin = new byte[TotalLength];
            var binSpan = new Span<byte>(bin);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan, Type);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[4..], TotalLength);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[8..], InterfaceID);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[12..], TimestampUpper);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[16..], TimestampLower);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[20..], CapturedPacketLength);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[24..], OriginalPacketLength);
            var pdataSpan = new Span<uint>(PacketData);
            var pdataBinSpan = MemoryMarshal.AsBytes(pdataSpan);
            pdataBinSpan.CopyTo(binSpan[28..]);

            int offset = 28 + 4 * PacketData.Length;
            foreach (var option in Options)
            {
                offset += option.WriteBytes(binSpan[offset..]);
            }

            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], opt_endofopt);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[(offset + 4)..], TotalLength2);
            return bin;
        }

        public EpbFlags Flags;
        public epb_flags FlagsOpt => new(Flags);
        public IEnumerable<epb_hash> Hash { get; set; } = [];
        public epb_dropcount? DropCount { get; set; } = null;
        public epb_packetid? PacketId { get; set; } = null;
        public epb_queue? Queue { get; set; } = null;
        public IEnumerable<epb_verdict> VerDict { get; set; } = [];
        public epb_processid_threadid? PidTid { get; set; } = null;
        public IEnumerable<opt_comment> Comments { get; set; } = [];
    }
}
