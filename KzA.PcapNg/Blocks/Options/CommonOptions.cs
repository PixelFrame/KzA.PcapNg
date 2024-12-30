using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks.Options
{
    internal class opt_endofopt : OptionBase
    {
        public override ushort Code { get => 0; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 0; set => throw new InvalidOperationException(); }
        public override uint[] Value { get => []; set => throw new InvalidOperationException(); }

        public override int Size => 4;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan, 0);
            return 4;
        }

        public override string PrintInfo()
        {
            return "opt_endofopt";
        }
    }

    public class opt_comment : Utf8StringOption
    {
        public override ushort Code { get => 1; set => throw new InvalidOperationException(); }

        public opt_comment() { }
        public opt_comment(string v) { _str = v; }

        public static implicit operator opt_comment(string v) => new(v);
    }
}
