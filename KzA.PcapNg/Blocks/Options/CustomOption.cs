﻿using KzA.PcapNg.Helper;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.Blocks.Options
{
    public class CustomOption : OptionBase
    {
        public override ushort Code { get; set; } = 0;
        public override ushort Length { get; set; } = 0;
        public uint PrivateEnterpriseNumber => Value[0];
        public override uint[] Value { get; set; } = [];
    }
}
