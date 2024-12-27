using KzA.PcapNg.DataTypes;
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

namespace KzA.PcapNg.Blocks
{
    public struct NameResolutionRecord
    {
        public NrbRecordType RecordType = 0;
        public ushort RecordValueLength = 0;
        public uint[] RecordValue = [];

        public readonly IPAddress IPAddress
        {
            get
            {
                if (RecordType == NrbRecordType.nrb_record_ipv4)
                {
                    return new IPAddress(RecordValue[0]);
                }
                else if (RecordType == NrbRecordType.nrb_record_ipv6)
                {
                    var valSpan = new Span<uint>(RecordValue);
                    var valBinSpan = MemoryMarshal.AsBytes(valSpan);
                    return new IPAddress(valBinSpan[..16]);
                }
                else
                {
                    throw new InvalidOperationException("RecordType is not an IP address");
                }
            }
        }

        public readonly string MACAddress
        {
            get
            {
                if (RecordType == NrbRecordType.nrb_record_eui48)
                {
                    return string.Join(":", RecordValue.Select(v => v.ToString("X2")));
                }
                else if (RecordType == NrbRecordType.nrb_record_eui64)
                {
                    return string.Join(":", RecordValue.Select(v => v.ToString("X2")));
                }
                else
                {
                    throw new InvalidOperationException("RecordType is not a MAC address");
                }
            }
        }

        public readonly string Name
        {
            get
            {
                var valSpan = new Span<uint>(RecordValue);
                var valBinSpan = MemoryMarshal.AsBytes(valSpan);
                return RecordType switch
                {
                    NrbRecordType.nrb_record_ipv4 => Encoding.UTF8.GetString(valBinSpan[4..(RecordValueLength - 1)]),
                    NrbRecordType.nrb_record_ipv6 => Encoding.UTF8.GetString(valBinSpan[16..(RecordValueLength - 1)]),
                    NrbRecordType.nrb_record_eui48 => Encoding.UTF8.GetString(valBinSpan[6..(RecordValueLength - 1)]),
                    NrbRecordType.nrb_record_eui64 => Encoding.UTF8.GetString(valBinSpan[8..(RecordValueLength - 1)]),
                    _ => string.Empty,
                };
            }
        }

        public NameResolutionRecord()
        {
        }

        public NameResolutionRecord(IPAddress addr, string name)
        {
            var strBytes = Encoding.UTF8.GetBytes(name);
            int strOffset;
            if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                RecordType = NrbRecordType.nrb_record_ipv4;
                RecordValueLength = (ushort)(4 + strBytes.Length);
                strOffset = 4;
            }
            else if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                RecordType = NrbRecordType.nrb_record_ipv6;
                RecordValueLength = (ushort)(16 + strBytes.Length);
                strOffset = 16;
            }
            else
            {
                throw new InvalidOperationException("Invalid IP address family");
            }

            var valLen = Misc.DwordPaddedDwLength(RecordValueLength);
            RecordValue = new uint[valLen];
            var valSpan = new Span<uint>(RecordValue);
            var valBinSpan = MemoryMarshal.AsBytes(valSpan);
            addr.GetAddressBytes().CopyTo(valBinSpan);
            strBytes.CopyTo(valBinSpan[strOffset..]);
        }

        public NameResolutionRecord(ReadOnlySpan<byte> mac, string name)
        {
            var strBytes = Encoding.UTF8.GetBytes(name);
            int strOffset;
            if (mac.Length == 6)
            {
                RecordType = NrbRecordType.nrb_record_eui48;
                RecordValueLength = (ushort)(6 + strBytes.Length);
                strOffset = 6;
            }
            else if (mac.Length == 8)
            {
                RecordType = NrbRecordType.nrb_record_eui64;
                RecordValueLength = (ushort)(8 + strBytes.Length);
                strOffset = 8;
            }
            else
            {
                throw new InvalidOperationException("Invalid MAC address length");
            }
            var valLen = Misc.DwordPaddedDwLength(RecordValueLength);
            RecordValue = new uint[valLen];
            var valSpan = new Span<uint>(RecordValue);
            var valBinSpan = MemoryMarshal.AsBytes(valSpan);
            mac.CopyTo(valBinSpan);
            strBytes.CopyTo(valBinSpan[strOffset..]);
        }

        public readonly int Size => 4 * (RecordValue.Length + 1);

        public readonly int WriteBytes(Span<byte> binSpan)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan, (ushort)RecordType);
            BinaryPrimitives.WriteUInt16LittleEndian(binSpan[2..], RecordValueLength);
            var valSpan = new Span<uint>(RecordValue);
            var valBinSpan = MemoryMarshal.AsBytes(valSpan);
            valBinSpan.CopyTo(binSpan[4..]);
            return Size;
        }

        public string PrintInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine(RecordType.ToString());
            sb.AppendLine($"RecordValueLength: {RecordValueLength}");
            sb.Append("Value: ");
            switch(RecordType)
            {
                case NrbRecordType.nrb_record_ipv4:
                case NrbRecordType.nrb_record_ipv6:
                    sb.AppendLine(IPAddress.ToString()); break;
                case NrbRecordType.nrb_record_eui48:
                case NrbRecordType.nrb_record_eui64:
                    sb.AppendLine(MACAddress); break;
            }
            return sb.ToString();
        }
    }
}
