using KzA.PcapNg.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.IO
{
    public class BlockWriter : IDisposable
    {
        private Stream _stream;
        private BinaryWriter _writer;
        private bool disposedValue;

        public BlockWriter(string path)
        {
            _stream = File.OpenWrite(path);
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

        ~BlockWriter()
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
