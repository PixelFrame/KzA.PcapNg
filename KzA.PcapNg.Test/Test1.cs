using KzA.PcapNg.Blocks;
using KzA.PcapNg.DataTypes;

namespace KzA.PcapNg.Test
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestPacketWrite0()
        {
            var section = new Section();
            section.Header.Comments = ["Test"];
            section.Header.Hardware = "Virtual PC";
            section.Header.OS = "M!uqoms 2064.13622.0.01";
            section.Header.UserAppl = "KzA.PcapNg";

            var inf = new InterfaceDescriptionBlock();
            inf.SnapLen = 0;
            inf.Name = "Ethernet";
            inf.Description = "Super Ethernet Adapter 1Tbps";
            inf.IPv4Addrs = [(0x0A010101u, 0xFFFFFF00)];
            inf.LinkType = LinkType.LINKTYPE_ETHERNET;
            inf.TsOffset = 0;
            inf.TsResol = (false, 6);

            section.Interfaces.Add(inf);

            var pkt = new EnhancedPacketBlock();
            pkt.InterfaceID = 0;
            var ts = new Timestamp(DateTime.Now);
            pkt.TimestampUpper = ts.Upper;
            pkt.TimestampLower = ts.Lower;
            pkt.WritePacketData(Convert.FromHexString(@"0017fa07c5280017fa06e1ea080045000069ea3e40003f067918ac1001040a202104e64c0d3dfdcd0e69f34eb25c80185ffb262000000101080a49f837d8009a3cc117030300300000000000004b3380677ee3c62fca9a1f6aeb64c76cb62e9c09fc4314e5f1d858474429575ea3127a23dabae584bd65"));

            section.EnhancedPackets.Add(pkt);

            section.AutoGenerateIsb();
            section.UpdateSectionLength();

            var pcapng = new PcapNg();
            pcapng.Sections.Add(section);

            pcapng.WriteAllSections("E:\\Temp\\test.pcapng");
        }

        [TestMethod]
        public void TestFileRead0()
        {
            var pcapng = new PcapNg();
            pcapng.ReadFile("E:\\Temp\\1.pcapng");
            // If last two blocks are correct, assuming the rest are correct :)
            Assert.IsTrue(pcapng.Sections[0].EnhancedPackets[200].TimestampUpper == 403723);
            Assert.IsTrue(pcapng.Sections[0].EnhancedPackets[200].TimestampLower == 3557021722);
            Assert.IsTrue(pcapng.Sections[0].InterfaceStatistics[0].Comments[0].StringValue == "Counters provided by dumpcap");
        }
    }
}
