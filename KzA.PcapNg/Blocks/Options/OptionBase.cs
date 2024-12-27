using KzA.PcapNg.Helper;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks.Options
{
    public abstract class OptionBase
    {
        abstract public ushort Code { get; set; }
        abstract public ushort Length { get; set; }
        abstract public uint[] Value { get; set; }

        public virtual int Size => Misc.DwordPaddedLength(Length) + 4;

        public virtual int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            var valSpan = new Span<uint>(Value);
            var valBinSpan = MemoryMarshal.AsBytes(valSpan);
            valBinSpan.CopyTo(binSpan[4..]);
            return Size;
        }

        // Should only be called by CustomOption
        internal virtual void Parse(ReadOnlySpan<byte> data, ushort code, ushort length)
        {
            Code = code;
            Length = length;
            Value = new uint[Misc.DwordPaddedDwLength(length)];
            var valueSpan = new Span<uint>(Value);
            var valueBinSpan = MemoryMarshal.AsBytes(valueSpan);
            data[..length].CopyTo(valueBinSpan);
        }

        public virtual string PrintInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{GetType().Name}({Code:X4})");
            sb.AppendLine($"Length: {Length}");
            sb.AppendLine($"Value: {string.Join(" ", Value)}");
            return sb.ToString();
        }
    }
}
