using KzA.PcapNg.Blocks;
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
        public SectionHeaderBlock Header { get; } = new();
        public List<InterfaceDescriptionBlock> Interfaces { get; } = [];
        public List<EnhancedPacketBlock> EnhancedPackets { get; } = [];
        public List<SimplePacketBlock> SimplePackets { get; } = [];
        public List<NameResolutionBlock> NameResolutions { get; } = [];
        public List<InterfaceStatisticsBlock> InterfaceStatistics { get; } = [];

    }
}
