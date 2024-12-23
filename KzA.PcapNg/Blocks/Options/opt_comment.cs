using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks.Options
{
    public class opt_comment : Utf8StringOption
    {
        public override ushort Code { get => 1; set => throw new InvalidOperationException(); }

        public opt_comment() { }
        public opt_comment(string v) { _str = v; }

        public static implicit operator opt_comment(string v) => new(v);
    }
}
