﻿using KzA.PcapNg.Blocks.Options;
using KzA.PcapNg.DataTypes;
using KzA.PcapNg.Helper;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks
{
    public class DecryptionSecretsBlock : IBlock
    {
        public uint Type => 0x00000006;
        public uint TotalLength => (uint)(4 * (5 + SecretsData.Length) + Options.Sum(o => o.Size));
        public SecretsType SecretsType { get; set; } = 0;
        public uint SecretsLength { get; set; } = 0;
        public uint[] SecretsData { get; set; } = [];
        public List<OptionBase> Options
        {
            get
            {
                var opts = new List<OptionBase>();
                if (Comments != null) opts.AddRange(Comments);
                if (CustomOptions != null) opts.AddRange(CustomOptions);
                if (opts.Count > 0) opts.Add(Misc.opt_endofopt);
                return opts;
            }
        }
        public uint TotalLength2 => TotalLength;

        public byte[] GetBytes()
        {
            var bin = new byte[TotalLength];
            var binSpan = new Span<byte>(bin);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan, Type);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[4..], TotalLength);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[8..], (uint)SecretsType);
            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[12..], SecretsLength);
            var sdataSpan = new Span<uint>(SecretsData);
            var sdataBinSpan = MemoryMarshal.AsBytes(sdataSpan);
            sdataBinSpan.CopyTo(binSpan[16..]);

            int offset = 16 + 4 * SecretsData.Length;
            foreach (var option in Options)
            {
                offset += option.WriteBytes(binSpan[offset..]);
            }

            BinaryPrimitives.WriteUInt32LittleEndian(binSpan[offset..], TotalLength2);
            return bin;
        }

        public void Parse(ReadOnlySpan<byte> data, uint totalLen, bool endian)
        {
            SecretsType = (SecretsType)(endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[8..]) : BinaryPrimitives.ReadUInt32BigEndian(data[8..]));
            SecretsLength = endian ? BinaryPrimitives.ReadUInt32LittleEndian(data[12..]) : BinaryPrimitives.ReadUInt32BigEndian(data[12..]);
            SecretsData = new uint[Misc.DwordPaddedDwLength(SecretsLength)];
            var sdataSpan = new Span<uint>(SecretsData);
            var sdataBinSpan = MemoryMarshal.AsBytes(sdataSpan);
            data[16..(int)(16 + SecretsLength)].CopyTo(sdataBinSpan);

            var offset = 16 + Misc.DwordPaddedLength(SecretsLength);
            var reachedEnd = false;
            while (offset < totalLen - 4 && !reachedEnd)
            {
                var code = endian ? BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]) : BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
                var length = endian ? BinaryPrimitives.ReadUInt16LittleEndian(data[(offset + 2)..]) : BinaryPrimitives.ReadUInt16BigEndian(data[(offset + 2)..]);
                switch (code)
                {
                    case 0x0000:
                        reachedEnd = true;
                        break;
                    case 0x0001:
                        Comments ??= [];
                        Comments.Add(Encoding.UTF8.GetString(data[(offset + 4)..(offset + 4 + length)]));
                        break;
                    default:
                        var customOption = new CustomOption();
                        customOption.Parse(data[offset..], code, length);
                        CustomOptions ??= [];
                        CustomOptions.Add(customOption);
                        break;
                }
                offset += Misc.DwordPaddedLength(length) + 4;
            }

        }

        public string PrintInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Decryption Secrets Block({Type:X8})");
            sb.AppendLine($"TotalLength: {TotalLength}");
            sb.AppendLine($"SecretsType: {SecretsType}");
            sb.AppendLine($"SecretsLength: {SecretsLength}");
            sb.AppendLine($"SecretsData");
            if (Options.Count > 0)
            {
                sb.AppendLine("Options:");
                foreach (var option in Options)
                {
                    sb.Append("  ");
                    sb.AppendLine(option.PrintInfo().Replace(Environment.NewLine, Environment.NewLine + "  "));
                }
            }
            return sb.ToString();
        }

        public List<opt_comment>? Comments { get; set; }
        public List<CustomOption>? CustomOptions { get; set; }
    }
}
