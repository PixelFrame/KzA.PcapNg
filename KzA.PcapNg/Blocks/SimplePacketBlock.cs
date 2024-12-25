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
    public class SimplePacketBlock : IBlock
    {
        public uint Type => 0x00000003;
        public uint TotalLength => (uint)(4 * (4 + PacketData.Length));
        public uint OriginalPacketLength { get; set; } = 0;
        public uint[] PacketData { get; set; } = [];
        public uint TotalLength2 => TotalLength;

        public void WritePacketData(byte[] data, uint originalLength = 0)
        {
            var capturedPacketLength = (uint)data.Length;
            if (originalLength < capturedPacketLength) OriginalPacketLength = capturedPacketLength;
            else OriginalPacketLength = originalLength;
            PacketData = new uint[Misc.DwordPaddedDwLength(capturedPacketLength)];
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
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[8..], OriginalPacketLength);
            var pdataSpan = new Span<uint>(PacketData);
            var pdataBinSpan = MemoryMarshal.AsBytes(pdataSpan);
            pdataBinSpan.CopyTo(binSpan[12..]);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[(12 + 4 * PacketData.Length)..], TotalLength2);
            return bin;
        }

        public void Parse(ReadOnlySpan<byte> data, uint totalLen, bool endian = true)
        {
            OriginalPacketLength = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[8..]) : BinaryPrimitives.ReadUInt32BigEndian(data[8..]);
            var packetDataLength = (totalLen - 16) / 4;
            PacketData = new uint[packetDataLength];
            var pdataSpan = new Span<uint>(PacketData);
            var pdataBinSpan = MemoryMarshal.AsBytes(pdataSpan);
            data[12..(int)(totalLen - 4)].CopyTo(pdataBinSpan);
        }
    }
}
