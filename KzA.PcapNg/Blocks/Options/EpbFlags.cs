using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KzA.PcapNg.Blocks.Options
{
    public struct EpbFlags
    {
        public uint Value;

        public DirectionEnum Direction
        {
            get => (DirectionEnum)(Value & 0x3);
            set
            {
                Value &= 0xFFFFFFFC;
                Value |= (byte)value;
            }
        }
        public ReceptionTypeEnum ReceptionType
        {
            get => (ReceptionTypeEnum)((Value & 0x1C) >> 2);
            set
            {
                Value &= 0xFFFFFFE3;
                Value |= (uint)value << 2;
            }
        }
        public byte FcsLen
        {
            get => (byte)((Value & 0x1E0) >> 5);
            set
            {
                if (value > 15) throw new ArgumentException("Value too large");
                Value &= 0xFFFFFE1F;
                Value |= (uint)value << 5;
            }
        }
        public bool ChecksumNotReady
        {
            get => (Value & 0x200) != 0;
            set
            {
                if (value) Value |= 0x200;
                else Value &= 0xFFFFFDFF;
            }
        }
        public bool ChecksumValid
        {
            get => (Value & 0x400) != 0;
            set
            {
                if (value) Value |= 0x400;
                else Value &= 0xFFFFFBFF;
            }
        }
        public bool TcpSegmentationOffloaded
        {
            get => (Value & 0x800) != 0;
            set
            {
                if (value) Value |= 0x800;
                else Value &= 0xFFFFF7FF;
            }
        }
        public LinkLayerErrorsEnum LinkLayerErrors
        {
            get => (LinkLayerErrorsEnum)(Value & 0xFFFF0000);
            set
            {
                Value &= 0x0000FFFF;
                Value |= (uint)value;
            }
        }

        public static implicit operator uint(EpbFlags f) => f.Value;
        public static explicit operator EpbFlags(uint u) => new() { Value = u };

        public enum DirectionEnum : byte
        {
            NotAvailable = 0,
            Inbound = 1,
            Outbound = 2,
        }
        public enum ReceptionTypeEnum : byte
        {
            NotSpecified = 0,
            Unicast = 1,
            Multicast = 2,
            Broadcast = 3,
            Promiscuous = 4,
        }
        [Flags] public enum LinkLayerErrorsEnum : uint
        {
            SymbolError = 0x80000000,
            PreambleError = 0x40000000,
            StartFrameDelimiterError = 0x20000000,
            UnalignedFrameError = 0x10000000,
            WrongInterFrameGapError = 0x08000000,
            PacketTooShortError = 0x04000000,
            PacketTooLongError = 0x02000000,
            CRCError = 0x01000000,
        }
    }
}
