using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks.Options
{
    public class shb_hardware : Utf8StringOption
    {
        public override ushort Code { get => 2; set => throw new InvalidOperationException(); }
        public shb_hardware() { }
        public shb_hardware(string v) { _str = v; }
        public static implicit operator shb_hardware(string v) => new(v);
    }

    public class shb_os : Utf8StringOption
    {
        public override ushort Code { get => 3; set => throw new InvalidOperationException(); }
        public shb_os() { }
        public shb_os(string v) { _str = v; }
        public static implicit operator shb_os(string v) => new(v);
    }

    public class shb_userappl : Utf8StringOption
    {
        public override ushort Code { get => 4; set => throw new InvalidOperationException(); }
        public shb_userappl() { }
        public shb_userappl(string v) { _str = v; }
        public static implicit operator shb_userappl(string v) => new(v);
    }
}
