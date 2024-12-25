using KzA.PcapNg.Blocks;
using KzA.PcapNg.IO;

namespace KzA.PcapNg
{
    public class PcapNg
    {
        public List<Section> Sections { get; } = [];

        public void WriteAllSections(string path)
        {
            using var writer = new PcapNgWriter(path);
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

        public void ReadFile(string path)
        {
            using var reader = new PcapNgReader(path);
            while (reader.PeekChar() != -1)
            {
                Sections.Add(reader.ReadSection());
            }
        }
    }
}
