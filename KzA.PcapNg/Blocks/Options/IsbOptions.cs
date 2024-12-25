using KzA.PcapNg.DataTypes;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks.Options
{
    public class isb_starttime : OptionBase
    {
        public uint Upper { get; set; }
        public uint Lower { get; set; }

        public override ushort Code { get => 2; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 8; set => throw new InvalidOperationException(); }
        public override uint[] Value { get => [Upper, Lower]; set => throw new InvalidOperationException(); }

        public override int Size => 12;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[4..], Upper);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[8..], Lower);
            return 12;
        }

        public isb_starttime() { }
        public isb_starttime(uint upper, uint lower) { Upper = upper; Lower = lower; }
        public static implicit operator isb_starttime((uint upper, uint lower) v) => new(v.upper, v.lower);
        public static implicit operator isb_starttime(Timestamp v) => new(v.Upper, v.Lower);

    }

    public class isb_endtime : OptionBase
    {
        public uint Upper { get; set; }
        public uint Lower { get; set; }

        public override ushort Code { get => 3; set => throw new InvalidOperationException(); }
        public override ushort Length { get => 8; set => throw new InvalidOperationException(); }
        public override uint[] Value { get => [Upper, Lower]; set => throw new InvalidOperationException(); }

        public override int Size => 12;

        public override int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, Code);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], Length);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[4..], Upper);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[8..], Lower);
            return 12;
        }

        public isb_endtime() { }
        public isb_endtime(uint upper, uint lower) { Upper = upper; Lower = lower; }
        public static implicit operator isb_endtime((uint upper, uint lower) v) => new(v.upper, v.lower);
        public static implicit operator isb_endtime(Timestamp v) => new(v.Upper, v.Lower);
    }

    public class isb_ifrecv : U64Option
    {
        public override ushort Code { get => 4; set => throw new InvalidOperationException(); }

        public isb_ifrecv() { }
        public isb_ifrecv(ulong value) { U64Value = value; }
        public static implicit operator isb_ifrecv(ulong v) => new(v);
    }

    public class isb_ifdrop : U64Option
    {
        public override ushort Code { get => 5; set => throw new InvalidOperationException(); }

        public isb_ifdrop() { }
        public isb_ifdrop(ulong value) { U64Value = value; }
        public static implicit operator isb_ifdrop(ulong v) => new(v);
    }

    public class isb_filteraccept : U64Option
    {
        public override ushort Code { get => 6; set => throw new InvalidOperationException(); }

        public isb_filteraccept() { }
        public isb_filteraccept(ulong value) { U64Value = value; }
        public static implicit operator isb_filteraccept(ulong v) => new(v);
    }

    public class isb_osdrop : U64Option
    {
        public override ushort Code { get => 7; set => throw new InvalidOperationException(); }

        public isb_osdrop() { }
        public isb_osdrop(ulong value) { U64Value = value; }
        public static implicit operator isb_osdrop(ulong v) => new(v);
    }

    public class isb_usrdeliv : U64Option
    {
        public override ushort Code { get => 8; set => throw new InvalidOperationException(); }

        public isb_usrdeliv() { }
        public isb_usrdeliv(ulong value) { U64Value = value; }
        public static implicit operator isb_usrdeliv(ulong v) => new(v);
    }
}
