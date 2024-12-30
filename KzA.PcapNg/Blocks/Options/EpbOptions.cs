using KzA.PcapNg.Helper;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks.Options
{
    public class epb_flags : OptionBase
    {
        public EpbFlags Flags { get; set; }
        public override ushort Code { get => 2; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 4; set => throw new InvalidOperationException(); }
        public override uint[] Value { get => [Flags]; set => throw new InvalidOperationException(); }

        public override int Size => 8;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[4..], Flags);
            return 8;
        }

        public epb_flags() { }
        public epb_flags(EpbFlags flags) { Flags = flags; }
        public epb_flags(uint flags) { Flags = (EpbFlags)flags; }
        public static implicit operator epb_flags(EpbFlags flags) => new(flags);
        public static implicit operator epb_flags(uint flags) => new(flags);

        public override string PrintInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"epb_flags(0004)");
            sb.AppendLine($"Length: {Length}");
            sb.AppendLine($"Value:");
            sb.AppendLine($"  Direction: {Flags.Direction}");
            sb.AppendLine($"  ReceptionType: {Flags.ReceptionType}");
            sb.AppendLine($"  FCS Length: {Flags.FcsLen}");
            sb.AppendLine($"  Checksum Not Ready: {Flags.ChecksumNotReady}");
            sb.AppendLine($"  Checksum Valid: {Flags.ChecksumValid}");
            sb.AppendLine($"  TCP Segmentation Offloaded: {Flags.TcpSegmentationOffloaded}");
            sb.AppendLine($"  Link-layer-dependent Errors: {Flags.LinkLayerErrors}");
            return sb.ToString();
        }
    }

    public class epb_hash : OptionBase
    {
        public byte Algorithm { get; set; }
        public byte[] Hash { get; set; } = [];

        public override ushort Code { get => 3; set => throw new InvalidOperationException(); }
        public override ushort Length { get => (ushort)(Hash.Length + 1); set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                var paddedLen = Misc.DwordPaddedDwLength(Length);
                var result = new uint[paddedLen];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                resultSpan[0] = Algorithm;
                new Span<byte>(Hash).CopyTo(resultSpan[1..]);
                return result;
            }
            set => throw new InvalidOperationException();
        }

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            binSpan[4] = Algorithm;
            new Span<byte>(Hash).CopyTo(binSpan[5..]);
            return Size;
        }

        public epb_hash() { }
        public epb_hash(byte algorithm, byte[] hash) { Algorithm = algorithm; Hash = hash; }
        public static implicit operator epb_hash((byte algorithm, byte[] hash) v) => new(v.algorithm, v.hash);
    }

    public class epb_dropcount : OptionBase
    {
        public ulong Count { get; set; }

        public override ushort Code { get => 4; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 8; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                var result = new uint[2];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                BinaryPrimitives.WriteUInt64LittleEndian(resultSpan, Count);
                return result;
            }
            set => throw new InvalidOperationException();
        }

        public override int Size => 12;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt64LittleEndian(binSpan[4..], Count);
            return 12;
        }

        public epb_dropcount() { }
        public epb_dropcount(ulong count) { Count = count; }
        public static implicit operator epb_dropcount(ulong v) => new(v);
    }

    public class epb_packetid : OptionBase
    {
        public ulong ID { get; set; }

        public override ushort Code { get => 5; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 8; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                var result = new uint[2];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                BinaryPrimitives.WriteUInt64LittleEndian(resultSpan, ID);
                return result;
            }
            set => throw new InvalidOperationException();
        }

        public override int Size => 12;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt64LittleEndian(binSpan[4..], ID);
            return 12;
        }

        public epb_packetid() { }
        public epb_packetid(ulong id) { ID = id; }
        public static implicit operator epb_packetid(ulong v) => new(v);
    }

    public class epb_queue : OptionBase
    {
        public uint Queue { get; set; }

        public override ushort Code { get => 6; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 4; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                return [Queue];
            }
            set => throw new InvalidOperationException();
        }

        public override int Size => 8;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[4..], Queue);
            return 8;
        }

        public epb_queue() { }
        public epb_queue(uint queue) { Queue = queue; }
        public static implicit operator epb_queue(uint v) => new(v);
    }

    public class epb_verdict : OptionBase
    {
        public byte Type { get; set; }
        public byte[] Data { get; set; } = [];

        public override ushort Code { get => 7; set => throw new InvalidOperationException(); }
        public override ushort Length { get => (ushort)(Data.Length + 1); set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get
            {
                var paddedLen = Misc.DwordPaddedDwLength(Length);
                var result = new uint[paddedLen];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                resultSpan[0] = Type;
                new Span<byte>(Data).CopyTo(resultSpan[1..]);
                return result;
            }
            set => throw new InvalidOperationException();
        }

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            binSpan[4] = Type;
            new Span<byte>(Data).CopyTo(binSpan[5..]);
            return Size;
        }

        public epb_verdict() { }
        public epb_verdict(byte type, byte[] data) { Type = type; Data = data; }
        public static implicit operator epb_verdict((byte type, byte[] data) v) => new(v.type, v.data);
    }

    public class epb_processid_threadid : OptionBase
    {
        public uint ProcessId { get; set; } = 0;
        public uint ThreadId { get; set; } = 0;

        public override ushort Code { get => 8; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 8; set => throw new InvalidOperationException(); }
        public override uint[] Value
        {
            get => [ProcessId, ThreadId];
            set => throw new InvalidOperationException();
        }
        public override int Size => 12;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[4..], ProcessId);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[8..], ThreadId);
            return 12;
        }

        public epb_processid_threadid() { }
        public epb_processid_threadid(uint processId, uint threadId) { ProcessId = processId; ThreadId = threadId; }
        public static implicit operator epb_processid_threadid((uint processId, uint threadId) v) => new(v.processId, v.threadId);
    }
}
