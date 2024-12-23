using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Helper
{
    internal static class Misc
    {
        public static int DwordPaddedLength(uint N)
        {
            return (int)((N + 3) & ~3);
        }

        public static uint DwordPaddedDwLength(uint N)
        {
            return (N + 3) >> 2;
        }
    }
}
