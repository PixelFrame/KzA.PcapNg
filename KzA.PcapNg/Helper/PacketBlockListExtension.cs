using KzA.PcapNg.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Helper
{
    internal static class PacketBlockListExtension
    {
        public static IEnumerable<EnhancedPacketBlock> GetLoadedPackets(this IEnumerable<EnhancedPacketBlock> blocks, int begin, int count)
        {
            foreach(var block in blocks.Skip(begin).Take(count))
            {
                block.LoadData();
                yield return block;
            }
        }

        public static IEnumerable<SimplePacketBlock> GetLoadedPackets(this IEnumerable<SimplePacketBlock> blocks, int begin, int count)
        {
            foreach (var block in blocks.Skip(begin).Take(count))
            {
                block.LoadData();
                yield return block;
            }
        }
    }
}
