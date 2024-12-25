using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks
{
    public interface IBlock
    {
        public uint Type { get; }
        public byte[] GetBytes();
        public void Parse(ReadOnlySpan<byte> data, uint totalLen, bool endian);
    }
}
