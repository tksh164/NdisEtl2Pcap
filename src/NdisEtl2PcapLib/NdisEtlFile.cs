using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using NdisEtl2PcapLib.NativeApi;
using NdisEtl2PcapLib.NativeApi.EventTracing;

namespace NdisEtl2PcapLib
{
    public sealed class NdisEtlFile : IDisposable
    {
        public string EtlFilePath { get; private set; }
        private ulong TraceHandle { get; set; }

        private List<NdisEventRecord> NdisEventRecords { get; set; }
        public NdisEventRecord[] Records { get { return NdisEventRecords.ToArray(); } }

        public long TotalEventRecordCount { get; private set; }
        public long TotalNdisEventRecordCount { get { return NdisEventRecords.LongCount(); } }

        public DateTime OldestNdisEventRecordTimestamp { get; private set; }
        public DateTime NewestNdisEventRecordTimestamp { get; private set; }

        private int CallCountOfBufferCallback { get; set; }
        private int CallCountOfEventRecordCallback { get; set; }
        private int CallCountOfEventRecordCallbackNdis { get; set; }

        public static NdisEtlFile Load(string etlFilePath)
        {
            var ndisEtlFile = new NdisEtlFile(etlFilePath);
            ndisEtlFile.ReadAllNdisEventRecords();
            return ndisEtlFile;
        }

        public void Dispose()
        {
            if (!EventTracingApi.IsInvalidProcessTraceHandle(TraceHandle))
            {
                EventTracingApi.CloseTrace(TraceHandle);
            }
        }

        private NdisEtlFile(string etlFilePath)
        {
            EtlFilePath = etlFilePath;
            NdisEventRecords = new List<NdisEventRecord>();
            TotalEventRecordCount = 0;
            OldestNdisEventRecordTimestamp = DateTime.MaxValue;
            NewestNdisEventRecordTimestamp = DateTime.MinValue;
        }

        private void ReadAllNdisEventRecords()
        {
            OpenEtlFile();
            ProcessEtlFile();
        }

        private EventRecordCallback EventRecordCallbackRetainer { get; set; }
        private BufferCallback BufferCallbackRetainer { get; set; }

        private void OpenEtlFile()
        {
            // Retained the delegate objects to avoid GC.
            EventRecordCallbackRetainer = new EventRecordCallback(EventRecordCallback);
            BufferCallbackRetainer = new BufferCallback(BufferCallback);

            // Open the ETL log file.
            EVENT_TRACE_LOGFILE traceLogFile = new EVENT_TRACE_LOGFILE
            {
                LogFileName = EtlFilePath,
                ProcessTraceMode = EventTracingApi.PROCESS_TRACE_MODE_EVENT_RECORD,
                BufferCallback = BufferCallbackRetainer,
                EventCallback = EventRecordCallbackRetainer,
            };
            TraceHandle = EventTracingApi.OpenTrace(ref traceLogFile);

            if (EventTracingApi.IsInvalidProcessTraceHandle(TraceHandle))
            {
                var win32ErrorCode = Marshal.GetLastWin32Error();

                string exceptionMessage;
                if (win32ErrorCode == Win32ErrorCode.ERROR_FILE_CORRUPT)
                {
                    exceptionMessage = string.Format("OpenTrace function failed. The file '{0}' is corrupted.", EtlFilePath);
                }
                else
                {
                    exceptionMessage = string.Format("OpenTrace function failed. The file tried to open was '{0}'.", EtlFilePath);
                }

                throw new Win32Exception(win32ErrorCode, exceptionMessage);
            }
        }

        private void ProcessEtlFile()
        {
            // Start trace processing.
            ulong[] traceHandles = new ulong[] { TraceHandle };
            uint result = EventTracingApi.ProcessTrace(traceHandles, (uint)traceHandles.Length, IntPtr.Zero, IntPtr.Zero);

            if (result != Win32ErrorCode.ERROR_SUCCESS)
            {
                throw new Win32Exception((int)result, string.Format("ProcessTrace function failed with 0x{0:x8}. The file that was open was '{1}'.", result, EtlFilePath));
            }
        }

        internal uint BufferCallback(ref EVENT_TRACE_LOGFILE buffer)
        {
            Debug.Write(nameof(BufferCallback));
            Debug.WriteLine("");

            CallCountOfBufferCallback++;
            return 1;
        }

        internal void EventRecordCallback(ref EVENT_RECORD eventRecord)
        {
            Debug.Write(nameof(EventRecordCallback));
            Debug.Write(string.Format(": TotalEventRecordCount: {0}", TotalEventRecordCount));
            Debug.WriteLine("");

            // for statistics.
            TotalEventRecordCount++;

            // We handle only the NDIS provider's record and packet fragment event.
            if (eventRecord.EventHeader.ProviderId == EventTracingApi.EtwNdisPacketCaptureProviderId &&
                eventRecord.EventHeader.EventDescriptor.Id == EventTracingApi.EtwNdisPacketFragmentRecordId)
            {
                var timestamp = GetNdisEventRecordTimestampUtc(eventRecord);
                var packetFragment = GetNdisEventRecordPacketFragment(eventRecord);
                NdisEventRecords.Add(new NdisEventRecord(timestamp, packetFragment));

                // for statistics.
                if (timestamp < OldestNdisEventRecordTimestamp)
                {
                    OldestNdisEventRecordTimestamp = timestamp;
                }

                if (timestamp > NewestNdisEventRecordTimestamp)
                {
                    NewestNdisEventRecordTimestamp = timestamp;
                }
            }
        }

        private static DateTime GetNdisEventRecordTimestampUtc(EVENT_RECORD eventRecord)
        {
            return DateTime.FromFileTime(eventRecord.EventHeader.TimeStamp);
        }

        private static byte[] GetNdisEventRecordPacketFragment(EVENT_RECORD eventRecord)
        {
            var userData = GetEventRecordUserData(eventRecord);
            var packetFragmentHeader = GetNdisEventRecordPacketFragmentHeader(userData);

            var packetFragment = new byte[packetFragmentHeader.FragmentSize];
            Buffer.BlockCopy(userData, Marshal.SizeOf<NdisEventRecordPacketFragmentHeader>(), packetFragment, 0, packetFragment.Length);
            return packetFragment;
        }

        private static byte[] GetEventRecordUserData(EVENT_RECORD eventRecord)
        {
            // Copy the unmanaged bytes to the managed byte array.
            var userData = new byte[eventRecord.UserDataLength];
            Marshal.Copy(eventRecord.UserData, userData, 0, userData.Length);
            return userData;
        }

        private static NdisEventRecordPacketFragmentHeader GetNdisEventRecordPacketFragmentHeader(byte[] eventRecordUserData)
        {
            // Allocate the user data object's handle and disable GC for the user data memory.
            var gcHandle = GCHandle.Alloc(eventRecordUserData, GCHandleType.Pinned);

            NdisEventRecordPacketFragmentHeader header;
            try
            {
                // Create a new header structure object from the head bytes of user data memory.
                header = (NdisEventRecordPacketFragmentHeader)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(NdisEventRecordPacketFragmentHeader));
            }
            finally
            {
                // Enable GC for the user data object.
                gcHandle.Free();
            }

            return header;
        }
    }
}
