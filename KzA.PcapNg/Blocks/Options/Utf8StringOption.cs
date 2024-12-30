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
    public abstract class Utf8StringOption : OptionBase
    {
        protected string _str = string.Empty;
        public string StringValue { get => _str; set => _str = value; }

        public override ushort Length
        {
            get => (ushort)Encoding.UTF8.GetByteCount(_str);
            set => throw new InvalidOperationException("Modify option value with StringValue property");
        }
        public override uint[] Value
        {
            get
            {
                var paddedLen = Misc.DwordPaddedDwLength(Length);
                var result = new uint[paddedLen];
                var resultSpan = MemoryMarshal.AsBytes(new Span<uint>(result));
                Encoding.UTF8.GetBytes(_str, resultSpan);
                return result;
            }
            set => throw new InvalidOperationException("Modify option value with StringValue property");
        }

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            Encoding.UTF8.GetBytes(_str, binSpan[4..]);
            return Size;
        }

        public override string PrintInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{GetType().Name}({Code:X4})");
            sb.AppendLine($"Length: {Length}");
            sb.AppendLine($"Value: {StringValue}");
            return sb.ToString();
        }
    }
}
