using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NdisEtl2PcapLib.Pcap
{
    public static class PcapFile
    {
        //
        // NPF structures and definitions
        // https://www.winpcap.org/docs/docs_412/html/group__NPF__include.html
        //
        // PCAP-DumpFileFormat
        // https://www.winpcap.org/ntar/draft/PCAP-DumpFileFormat_ts.html
        //

        // pcap file header contants.
        private const uint TCPDUMP_MAGIC = 0xa1b2c3d4;  // pcap file header magic.
        private const ushort PCAP_VERSION_MAJOR = 2;
        private const ushort PCAP_VERSION_MINOR = 4;
        private const uint PcapFileHeaderSnapLengthValue = 262144;  // Wireshark used this value. Reason is unknown.
        private const uint LINKTYPE_ETHERNET = 1;

        // pcap_file_header structure
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct PcapFileHeader
        {
            public uint Magic;              // Magic.
            public ushort VersionMajor;     // Libpcap major version.
            public ushort VersionMinor;     // Libpcap minor version.
            public int ThisZone;            // GMT to local correction.
            public uint Sigfigs;            // accuracy of timestamps.
            public uint SnapLength;         // Max length saved portion of each packet.
            public uint LinkType;           // Data link type.
        }

        // timeval structure
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TimeInterval
        {
            public int Seconds;
            public int Microseconds;
        }

        // sf_pkthdr structure
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct PacketHeader
        {
            public TimeInterval Timestamp;      // Timestamp
            public uint CapturedPacketLength;   // Length of captured portion.
            public uint OriginalPacketLength;   // Length of the original packet (off wire).
        }

        public static void WritePcapFile(string pcapFilePath, NdisEventRecord[] packets)
        {
            using (var stream = new FileStream(pcapFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new BinaryWriter(stream))
            {
                WriteFileHeader(writer);

                foreach (var packet in packets)
                {
                    WritePacket(writer, packet);
                }
            }
        }

        private static void WriteFileHeader(BinaryWriter writer)
        {
            PcapFileHeader pcapFileHeader = new PcapFileHeader()
            {
                Magic = TCPDUMP_MAGIC,
                VersionMajor = PCAP_VERSION_MAJOR,
                VersionMinor = PCAP_VERSION_MINOR,
                ThisZone = 0,
                Sigfigs = 0,
                SnapLength = PcapFileHeaderSnapLengthValue,
                LinkType = LINKTYPE_ETHERNET,
            };

            var buffer = GetStructAsByteArray(pcapFileHeader);
            writer.Write(buffer);
        }

        private static void WritePacket(BinaryWriter writer, NdisEventRecord capturedPacket)
        {
            PacketHeader packetHeader = new PacketHeader()
            {
                Timestamp = GetTimeInterval(capturedPacket.TimestampUtc),
                CapturedPacketLength = (uint)capturedPacket.PacketFragment.Length,
                OriginalPacketLength = (uint)capturedPacket.PacketFragment.Length,
            };

            var buffer = GetStructAsByteArray(packetHeader);
            writer.Write(buffer);

            writer.Write(capturedPacket.PacketFragment);
        }

        private static TimeInterval GetTimeInterval(DateTime timestamp)
        {
            var unixTimeTicks = timestamp.Subtract(new DateTime(1970, 1, 1)).Ticks;
            var seconds = (int)(unixTimeTicks / TimeSpan.TicksPerSecond);
            var microseconds = (int)((unixTimeTicks % TimeSpan.TicksPerSecond) / 10);
            return new TimeInterval()
            {
                Seconds = seconds,
                Microseconds = microseconds,
            };
        }

        private static byte[] GetStructAsByteArray<T>(T structure)
        {
            // Create a byte array object for the buffer.
            var sizeOfStruct = Marshal.SizeOf<T>(structure);
            var buffer = new byte[sizeOfStruct];

            // Allocate the buffer's handle and disable GC for that memory.
            var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                // Copy the structure bytes to the allocated buffer.
                Marshal.StructureToPtr<T>(structure, gcHandle.AddrOfPinnedObject(), false);
            }
            finally
            {
                // Enable GC for the buffer memory.
                gcHandle.Free();
            }

            return buffer;
        }
    }
}
