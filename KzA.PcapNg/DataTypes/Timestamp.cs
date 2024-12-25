using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.DataTypes
{
    public struct Timestamp : IComparable<Timestamp>
    {
        private static readonly byte[] supportedResol = [0, 1, 2, 3, 4, 5, 6, 7];

        public uint Upper;
        public uint Lower;
        public readonly ulong Value => (ulong)Upper << 32 | Lower;
        public long Offset;
        public bool BaseBit;
        public byte Exponent;

        public Timestamp(DateTime t, long offset = 0, byte exponent = 6)
        {
            Offset = offset;
            BaseBit = false;
            Exponent = exponent;
            DateTime = t;
        }

        public Timestamp(uint upper, uint lower, long offset = 0, bool baseBit = false, byte exponent = 6)
        {
            Upper = upper;
            Lower = lower;
            Offset = offset;
            BaseBit = baseBit;
            Exponent = exponent;
        }

        public DateTime DateTime
        {
            readonly get
            {;
                var baseTime = DateTime.UnixEpoch.AddSeconds(Offset);
                if (BaseBit)
                {
                    return baseTime.AddSeconds(Value * Math.Pow(2, Exponent));
                }
                else
                {
                    return baseTime.AddTicks((long)(Value * Math.Pow(10, 7 - Exponent)));
                }
            }
            set
            {
                if (BaseBit || !supportedResol.Contains(Exponent))
                {
                    throw new NotSupportedException("Only support 10-based resolution up to ticks");
                }
                var ticks = (ulong)(value.ToUniversalTime().Ticks - (DateTime.UnixEpoch.AddSeconds(Offset)).Ticks);
                var v = ticks / (ulong)Math.Pow(10, 7 - Exponent);
                Upper = (uint)(v >> 32);
                Lower = (uint)v;
            }
        }

        public readonly Timestamp ToTickBased()
        {
            return new Timestamp(DateTime, Offset, 7);
        }

        public readonly Timestamp ToMicrosecondBased()
        {
            return new Timestamp(DateTime, Offset, 6);
        }

        public readonly Timestamp ToMillisecondBased()
        {
            return new Timestamp(DateTime, Offset, 3);
        }

        public readonly int CompareTo(Timestamp other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}
