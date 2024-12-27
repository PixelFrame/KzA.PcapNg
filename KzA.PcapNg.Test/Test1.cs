using KzA.PcapNg.Blocks;
using KzA.PcapNg.DataTypes;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

            pcapng.WriteFile(@"..\..\..\TestCap\Generated0.pcapng");
            // TODO: Validate the file with tshark
        }

        [TestMethod]
        public void TestFileRead0()
        {
            var pcapng = new PcapNg();
            pcapng.ReadFile(@"..\..\..\TestCap\Input0.pcapng");
            // If last two blocks are correct, assuming the rest are correct :)
            Assert.IsTrue(pcapng.Sections[0].EnhancedPackets[9].TimestampUpper == 403989);
            Assert.IsTrue(pcapng.Sections[0].EnhancedPackets[9].TimestampLower == 1805473145);
            Assert.IsTrue(pcapng.Sections[0].InterfaceStatistics[0].Comments![0].StringValue == "Counters provided by dumpcap");
        }

        // Large file memory test
        // The test file should have at least 300000 packets
        [TestMethod]
        public void TestFileRead1()
        {
            var pcapng = new PcapNg(true);
            Console.WriteLine($"Initial Memory: {Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0}MB");
            pcapng.ReadFile(@"..\..\..\TestCap\Input1.large.pcapng");
            //Memory when no packet is loaded
            Console.WriteLine($"Blocks Parsed Memory: {Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0}MB");

            //Load first 100000 packets
            pcapng.Sections[0].LoadPackets(0, 100000, true);
            var data = MemoryMarshal.AsBytes(new Span<uint>(pcapng.Sections[0].EnhancedPackets[5000].PacketData));
            foreach (var b in data)
            {
                Console.Write(b.ToString("X2"));
            }
            Console.WriteLine();
            Console.WriteLine($"0-100000 Loaded Memory: {Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0}MB");
            //Release first 100000 packets
            pcapng.Sections[0].UnloadAllPackets();
            Console.WriteLine($"0-99999 Unloaded Memory: {Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0}MB");

            //Load next 100000 packets
            pcapng.Sections[0].LoadPackets(100000, 100000, true);
            data = MemoryMarshal.AsBytes(new Span<uint>(pcapng.Sections[0].EnhancedPackets[150000].PacketData));
            foreach (var b in data)
            {
                Console.Write(b.ToString("X2"));
            }
            Console.WriteLine();
            Console.WriteLine($"100000-199999 Loaded Memory: {Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0}MB");
            // Release next 100000 packets
            pcapng.Sections[0].UnloadAllPackets();
            Console.WriteLine($"100000-199999 Unloaded Memory: {Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0}MB");

            //Load last 100000 packets
            pcapng.Sections[0].LoadPackets(200000, 100000, true);
            data = MemoryMarshal.AsBytes(new Span<uint>(pcapng.Sections[0].EnhancedPackets[250000].PacketData));
            foreach (var b in data)
            {
                Console.Write(b.ToString("X2"));
            }
            Console.WriteLine();
            Console.WriteLine($"200000-299999 Loaded Memory: {Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0}MB");

            //Release last 100000 packets
            pcapng.Sections[0].UnloadAllPackets();
            Console.WriteLine($"200000-299999 Unloaded Memory: {Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0}MB");

            pcapng.Dispose();
        }


        [TestMethod]
        public void TestFileRead2()
        {
            var pcapng = new PcapNg();
            pcapng.ReadFile(@"..\..\..\TestCap\Input2.tlskey.pcapng");
            Assert.IsTrue(pcapng.Sections[0].DecryptionSecrets[0].SecretsLength == 13392);
            Assert.IsTrue(pcapng.Sections[0].DecryptionSecrets[0].SecretsType == SecretsType.TLSKeyLog);
        }

        [TestMethod]
        public void TestFileRead3()
        {
            var pcapng = new PcapNg();
            pcapng.ReadFile(@"..\..\..\TestCap\Input3.pcapng");
            Assert.IsTrue(pcapng.Sections[0].NameResolutions[0].Records[0].RecordType == NrbRecordType.nrb_record_ipv4);
            Assert.IsTrue(pcapng.Sections[0].NameResolutions[0].Records[0].Name == "CLIENT");
            Assert.IsTrue(pcapng.Sections[0].NameResolutions[0].Records[3].RecordType == NrbRecordType.nrb_record_ipv6);
            Assert.IsTrue(pcapng.Sections[0].NameResolutions[0].Records[3].Name == "SERVER6");
        }

        [TestMethod]
        public void Relog()
        {
            var pcapng = new PcapNg(true);
            pcapng.ReadFile(@"..\..\..\TestCap\Input1.large.pcapng");
            pcapng.WriteFile(@"..\..\..\TestCap\Relog1.large.pcapng");
        }
    }
}
