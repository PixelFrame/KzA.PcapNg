using KzA.PcapNg.Blocks;
using KzA.PcapNg.IO;

namespace KzA.PcapNg
{
    public class PcapNg : IDisposable
    {
        public List<Section> Sections { get; } = [];
        private bool LazyLoadMode = false;
        private bool disposedValue;

        public PcapNg(bool lazyLoadMode = false)
        {
            LazyLoadMode = lazyLoadMode;
        }

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
            using var reader = new PcapNgReader(path, LazyLoadMode);
            while (reader.PeekChar() != -1)
            {
                Sections.Add(reader.ReadSection());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Sections[0].Stream?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
