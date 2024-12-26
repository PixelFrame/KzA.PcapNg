using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.DataTypes
{
    public enum SecretsType : uint
    {
        SSHKeyLog = 0x5353484b,
        TLSKeyLog = 0x544c534b,
        OPCUAKeyLog = 0x55414b4c,
        WireGuardKeyLog = 0x57474b4c,
        ZigBeeNWKKey = 0x5a4e574b,
        ZigBeeAPSKey = 0x5a415053,
    }
}
