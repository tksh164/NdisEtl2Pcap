using System;

namespace NdisEtl2PcapLib
{
    public sealed class NdisEventRecord
    {
        public DateTime TimestampUtc { get; private set; }
        public byte[] PacketFragment { get; private set; }

        internal NdisEventRecord(DateTime timestampUtc, byte[] packetFragment)
        {
            TimestampUtc = timestampUtc;
            PacketFragment = packetFragment;
        }
    }
}
