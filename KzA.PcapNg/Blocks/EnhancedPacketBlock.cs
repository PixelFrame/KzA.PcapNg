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
using System.Xml.Linq;

namespace KzA.PcapNg.Blocks
{
    public class EnhancedPacketBlock : IBlock, IComparable<EnhancedPacketBlock>
    {
        private readonly Section? section;
        internal bool IsDataLoaded => packetData.Length != 0;
        private long? position = 0;

        public uint Type => 0x00000006;
        public uint TotalLength => (uint)(4 * (9 + PacketData.Length) + Options.Sum(o => o.Size));
        public uint InterfaceID { get; set; } = 0;
        public uint TimestampUpper { get; set; } = 0;
        public uint TimestampLower { get; set; } = 0;
        public Timestamp Timestamp => new(TimestampUpper, TimestampLower);
        public uint CapturedPacketLength { get; set; } = 0;
        public uint OriginalPacketLength { get; set; } = 0;
        private uint[] packetData { get; set; } = [];
        public uint[] PacketData
        {
            get
            {
                if(!IsDataLoaded)
                {
                    LoadData();
                }
                return packetData;
            }
            set
            {
                packetData = value;
            }
        }
        public List<OptionBase> Options
        {
            get
            {
                var opts = new List<OptionBase>();
                if (Flags != null) opts.Add(Flags);
                if (Hash != null) opts.AddRange(Hash);
                if (DropCount != null) opts.Add(DropCount);
                if (PacketId != null) opts.Add(PacketId);
                if (Queue != null) opts.Add(Queue);
                if (VerDict != null) opts.AddRange(VerDict);
                if (PidTid != null) opts.Add(PidTid);
                if (Comments != null) opts.AddRange(Comments);
                if (CustomOptions != null) opts.AddRange(CustomOptions);
                return opts;
            }
        }
        public uint opt_endofopt => 0;
        public uint TotalLength2 => TotalLength;

        public EnhancedPacketBlock() { }
        public EnhancedPacketBlock(Section section)
        {
            this.section = section;
        }

        public void WritePacketData(byte[] data, uint originalLength = 0)
        {
            CapturedPacketLength = (uint)data.Length;
            if (originalLength < CapturedPacketLength) OriginalPacketLength = CapturedPacketLength;
            else OriginalPacketLength = originalLength;
            PacketData = new uint[Misc.DwordPaddedDwLength(CapturedPacketLength)];
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

        public void Parse(ReadOnlySpan<byte> data, uint totalLen, bool endian = true)
        {
            InterfaceID = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[8..]) : BinaryPrimitives.ReadUInt32BigEndian(data[8..]);
            TimestampUpper = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[12..]) : BinaryPrimitives.ReadUInt32BigEndian(data[12..]);
            TimestampLower = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[16..]) : BinaryPrimitives.ReadUInt32BigEndian(data[16..]);
            CapturedPacketLength = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[20..]) : BinaryPrimitives.ReadUInt32BigEndian(data[20..]);
            OriginalPacketLength = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[24..]) : BinaryPrimitives.ReadUInt32BigEndian(data[24..]);
            if (section == null)
            {
                PacketData = new uint[Misc.DwordPaddedDwLength(CapturedPacketLength)];
                var pdataSpan = new Span<uint>(PacketData);
                var pdataBinSpan = MemoryMarshal.AsBytes(pdataSpan);
                data[28..(int)(28 + CapturedPacketLength)].CopyTo(pdataBinSpan);
            }
            else
            {
                position = section.Stream!.Position - totalLen + 28;
            }
            var offset = 28 + Misc.DwordPaddedLength(CapturedPacketLength);
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
                        Flags = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[(offset + 4)..(offset + 8)]) : BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 4)..(offset + 8)]);
                        break;
                    case 0x0003:
                        Hash ??= [];
                        Hash.Add((data[offset + 4], data[(offset + 5)..(offset + length)].ToArray()));
                        break;
                    case 0x0004:
                        DropCount = endian ? BinaryPrimitives.ReadUInt64LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt64BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x0005:
                        PacketId = endian ? BinaryPrimitives.ReadUInt64LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt64BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x0006:
                        Queue = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 4)..]);
                        break;
                    case 0x0007:
                        VerDict ??= [];
                        VerDict.Add((data[offset + 4], data[(offset + 5)..(offset + length)].ToArray()));
                        break;
                    case 0x0008:
                        PidTid = (endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[(offset + 4)..]) : BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 4)..]),
                            endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[(offset + 8)..]) : BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 8)..]));
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

        public int CompareTo(EnhancedPacketBlock? other)
        {
            if (other == null) return 1;
            return Timestamp.CompareTo(other.Timestamp);
        }

        internal void LoadData()
        {
            if (section == null)
            {
                throw new Exception("Cannot load data without section");
            }
            section.Stream!.Seek(position!.Value, SeekOrigin.Begin);
            packetData = new uint[Misc.DwordPaddedDwLength(CapturedPacketLength)];
            var pdataSpan = new Span<uint>(packetData);
            var pdataBinSpan = MemoryMarshal.AsBytes(pdataSpan);
            section.Stream.ReadExactly(pdataBinSpan);
        }

        internal void UnloadData()
        {
            packetData = [];
        }

        public epb_flags? Flags = null;
        public List<epb_hash>? Hash { get; set; }
        public epb_dropcount? DropCount { get; set; } = null;
        public epb_packetid? PacketId { get; set; } = null;
        public epb_queue? Queue { get; set; } = null;
        public List<epb_verdict>? VerDict { get; set; }
        public epb_processid_threadid? PidTid { get; set; } = null;
        public List<opt_comment>? Comments { get; set; }
        public List<CustomOption>? CustomOptions { get; set; }
    }
}
