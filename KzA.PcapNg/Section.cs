﻿using KzA.PcapNg.Blocks;
using KzA.PcapNg.Blocks.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg
{
    public class Section
    {
        public bool LazyDataLoad => Stream != null;
        internal readonly Stream? Stream;
        public SectionHeaderBlock Header { get; } = new();
        public List<InterfaceDescriptionBlock> Interfaces { get; } = [];
        public List<EnhancedPacketBlock> EnhancedPackets { get; } = [];
        public List<SimplePacketBlock> SimplePackets { get; } = [];
        public List<NameResolutionBlock> NameResolutions { get; } = [];
        public List<InterfaceStatisticsBlock> InterfaceStatistics { get; } = [];

        public Section() { }
        public Section(Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("Stream must be seekable for lazy loading");
            }
            Stream = stream;
        }

        public void AutoGenerateIsb(bool SkipZeroPacketIf = true, bool SuppressComment = false)
        {
            for (uint i = 0; i < Interfaces.Count; i++)
            {
                var pkts = EnhancedPackets.Where(p => p.InterfaceID == i);
                if (SkipZeroPacketIf && !pkts.Any()) continue;

                pkts = pkts.Order();
                var isb = new InterfaceStatisticsBlock
                {
                    InterfaceID = i,
                    StartTime = pkts.First().Timestamp,
                    EndTime = pkts.Last().Timestamp,
                    IfRecv = (uint)pkts.Count(),
                    IfDrop = 0
                };
                isb.TimestampUpper = isb.EndTime.Upper;
                isb.TimestampLower = isb.EndTime.Lower;
                if (!SuppressComment) isb.Comments = ["Auto generated by KzA.PcapNg"];

                InterfaceStatistics.Add(isb);
            }
        }

        public void UpdateSectionLength()
        {
            Header.SectionLength = Interfaces.Sum(i => i.TotalLength)
                + EnhancedPackets.Sum(p => p.TotalLength)
                + SimplePackets.Sum(p => p.TotalLength)
                + NameResolutions.Sum(n => n.TotalLength)
                + InterfaceStatistics.Sum(s => s.TotalLength);
        }

        public void LoadPackets(int begin, int count, bool enhancedOrSimple)
        {
            if (enhancedOrSimple)
            {
                EnhancedPackets.Skip(begin).Take(count).ToList().ForEach(p => p.LoadData());
            }
            else
            {
                SimplePackets.Skip(begin).Take(count).ToList().ForEach(p => p.LoadData());
            }
        }

        public void UnloadAllPackets(bool performGC = true)
        {
            EnhancedPackets.ForEach(p => p.UnloadData());
            SimplePackets.ForEach(p => p.UnloadData());
            if (performGC) GC.Collect();
        }
    }
}
