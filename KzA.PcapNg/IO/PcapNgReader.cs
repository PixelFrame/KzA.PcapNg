﻿using KzA.PcapNg.Blocks;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.PcapNg.IO
{
    public class PcapNgReader : IDisposable
    {
        private readonly BinaryReader _reader;
        private bool disposedValue;
        private bool currentEndian;
        private PcapNg? pcapNg;

        public int PeekChar() => _reader.PeekChar();


        public PcapNgReader(Stream stream, PcapNg? pcapNg)
        {
            _reader = new(stream);
            this.pcapNg = pcapNg;
        }

        public Section ReadSection()
        {
            var section = pcapNg != null ? new Section(pcapNg) : new Section();
            section.Header.Parse(_reader);
            currentEndian = section.Header.LittleEndian;
            bool reachedNextSection = false;
            while (_reader.PeekChar() != -1 || reachedNextSection)
            {
                var (type, length) = PeekBlock();
                var buffer = _reader.ReadBytes((int)length);
                switch (type)
                {
                    case 0x00000001:
                        var idb = new InterfaceDescriptionBlock();
                        idb.Parse(buffer, length, currentEndian);
                        section.Interfaces.Add(idb);
                        break;
                    case 0x00000003:
                        var spb = pcapNg != null ? new SimplePacketBlock(section) : new SimplePacketBlock();
                        spb.Parse(buffer, length, currentEndian);
                        section.SimplePackets.Add(spb);
                        break;
                    case 0x00000004:
                        var nrb = new NameResolutionBlock();
                        nrb.Parse(buffer, length, currentEndian);
                        section.NameResolutions.Add(nrb);
                        break;
                    case 0x00000005:
                        var isb = new InterfaceStatisticsBlock();
                        isb.Parse(buffer, length, currentEndian);
                        section.InterfaceStatistics.Add(isb);
                        break;
                    case 0x00000006:
                        var epb = pcapNg != null ? new EnhancedPacketBlock(section) : new EnhancedPacketBlock();
                        epb.Parse(buffer, length, currentEndian);
                        section.EnhancedPackets.Add(epb);
                        break;
                    case 0x0000000A:
                        var dsb = new DecryptionSecretsBlock();
                        dsb.Parse(buffer, length, currentEndian);
                        section.DecryptionSecrets.Add(dsb);
                        break;
                    case 0x00000BAD:
                    case 0x40000BAD:
                        var cb = new CustomBlock();
                        cb.Parse(buffer, length, currentEndian);
                        section.CustomBlocks.Add(cb);
                        break;
                    case 0x0A0D0D0A:
                        reachedNextSection = true;
                        break;
                }
            }
            return section;
        }

        private (uint type, uint length) PeekBlock()
        {
            var type = _reader.ReadUInt32();
            var length = _reader.ReadUInt32();
            if (!currentEndian)
            {
                type = BinaryPrimitives.ReverseEndianness(type);
                length = BinaryPrimitives.ReverseEndianness(length);
            }
            _reader.BaseStream.Seek(-8, SeekOrigin.Current);
            return (type, length);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (pcapNg != null) return;
            if (!disposedValue)
            {
                if (disposing)
                {
                    _reader.Dispose();
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
