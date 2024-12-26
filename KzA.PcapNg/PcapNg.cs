using KzA.PcapNg.Blocks;
using KzA.PcapNg.Helper;
using KzA.PcapNg.IO;

namespace KzA.PcapNg
{
    public class PcapNg : IDisposable
    {
        public List<Section> Sections { get; } = [];
        private bool lazyLoadMode = false;
        public bool LazyLoadMode => lazyLoadMode;
        internal Stream? Stream;
        private bool disposedValue;

        public PcapNg(bool lazyLoadMode = false)
        {
            this.lazyLoadMode = lazyLoadMode;
        }

        public void LoadNow(bool releaseStream)
        {
            lazyLoadMode = false;
            Sections.ForEach(s => s.LoadAllPackets());
            if (releaseStream && Sections.Count > 0) Sections[0].Stream?.Dispose();
        }

        public void WriteFile(string path)
        {
            using var writer = new PcapNgWriter(path);
            foreach (var section in Sections)
            {
                writer.Write(section.Header);
                writer.WriteAll(section.Interfaces);
                writer.WriteAll(section.DecryptionSecrets);
                if (!lazyLoadMode)
                {
                    writer.WriteAll(section.EnhancedPackets);
                    writer.WriteAll(section.SimplePackets);
                }
                else
                {
                    var cnt = 0;
                    while (cnt < section.EnhancedPackets.Count)
                    {
                        var loadedPackets = section.EnhancedPackets.GetLoadedPackets(cnt, 100000);
                        writer.WriteAll(loadedPackets);
                        cnt += 100000;
                        foreach (var pkt in loadedPackets)
                        {
                            pkt.UnloadData();
                        }
                    }
                    cnt = 0;
                    while (cnt < section.SimplePackets.Count)
                    {
                        var loadedPackets = section.SimplePackets.GetLoadedPackets(cnt, 100000);
                        writer.WriteAll(loadedPackets);
                        cnt += 100000;
                        foreach (var pkt in loadedPackets)
                        {
                            pkt.UnloadData();
                        }
                    }
                }
                writer.WriteAll(section.NameResolutions);
                writer.WriteAll(section.InterfaceStatistics);
                writer.WriteAll(section.CustomBlocks);
            }
        }

        public void ReadFile(string path)
        {
            if (Sections.Count > 0 || Stream != null)
            {
                throw new InvalidOperationException("File already read, create a new instance for another file");
            }
            Stream = File.OpenRead(path);
            using var reader = lazyLoadMode ? new PcapNgReader(Stream, this) : new PcapNgReader(Stream, null);
            while (reader.PeekChar() != -1)
            {
                Sections.Add(reader.ReadSection());
            }
        }

        public void ReadStream(Stream stream)
        {
            if (Sections.Count > 0 || Stream != null)
            {
                throw new InvalidOperationException("Stream already read, create a new instance for another stream");
            }
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("Stream must be seekable");
            }
            if (!stream.CanRead)
            {
                throw new InvalidOperationException("Stream must be readable");
            }
            Stream = stream;
            using var reader = lazyLoadMode ? new PcapNgReader(Stream, this) : new PcapNgReader(Stream, null);
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
                    Stream?.Dispose();
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
