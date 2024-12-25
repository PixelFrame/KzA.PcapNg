using KzA.PcapNg.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.IO
{
    public class PcapNgWriter : IDisposable
    {
        private readonly Stream _stream;
        private readonly BinaryWriter _writer;
        private bool disposedValue;

        public PcapNgWriter(string path, bool removeExisting = true)
        {
            if (File.Exists(path))
            {
                if(removeExisting) File.Delete(path);
                else throw new IOException("File already exists");
            }
            _stream = File.Open(path, FileMode.CreateNew);
            _writer = new(_stream);
        }

        public void Write(IBlock block)
        {
            _writer.Write(block.GetBytes());
        }

        public void WriteAll(IEnumerable<IBlock> blocks)
        {
            foreach (var block in blocks)
            {
                _writer.Write(block.GetBytes());
            }
        }

        ~PcapNgWriter()
        {
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _writer.Dispose();
                    _stream.Dispose();
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
