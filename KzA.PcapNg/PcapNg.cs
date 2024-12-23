using KzA.PcapNg.Blocks;
using KzA.PcapNg.IO;

namespace KzA.PcapNg
{
    public class PcapNg
    {
        public List<Section> Sections { get; } = [];

        public void WriteAllSections(string path)
        {
            using var writer = new BlockWriter(path);
            foreach (var section in Sections)
            {
                writer.Write(section.Header);
                writer.WriteAll(section.Interfaces);
                writer.WriteAll(section.EnhancedPackets);
                writer.WriteAll(section.SimplePackets);
                writer.WriteAll(section.NameResolutions);
                writer.WriteAll(section.InterfaceStatistics);
            }
        }
    }
}
