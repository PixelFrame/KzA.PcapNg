using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.DataTypes
{
    public enum NrbRecordType : ushort
    {
        nrb_record_end = 0,
        nrb_record_ipv4 = 1,
        nrb_record_ipv6 = 2,
        nrb_record_eui48 = 3,
        nrb_record_eui64 = 4,
    }
}
