using System;
using System.Runtime.InteropServices;

namespace NdisEtl2PcapLib.NativeApi.EventTracing
{
    //
    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms724950(v=vs.85).aspx
    //

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEMTIME
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }

    //
    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms725481(v=vs.85).aspx
    //

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct TIME_ZONE_INFORMATION
    {
        public int Bias;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string StandardName;

        public SYSTEMTIME StandardDate;
        public int StandardBias;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DaylightName;

        public SYSTEMTIME DaylightDate;
        public int DaylightBias;
    }

    //
    // https://docs.microsoft.com/en-us/windows/desktop/etw/event-trace-header
    //

    [StructLayout(LayoutKind.Sequential)]
    internal struct EVENT_TRACE_HEADER
    {
        public ushort Size;
        public ushort FieldTypeFlags;   // Reserved.

        public byte ClassType;
        public byte ClassLevel;
        public byte ClassVersion;

        public uint ThreadId;
        public uint ProcessId;
        public ulong TimeStamp;

        public Guid Guid;

        public uint ClientContext;      // Reserved.
        public uint Flags;
    }

    //
    // https://docs.microsoft.com/en-us/windows/desktop/etw/event-trace
    //

    [StructLayout(LayoutKind.Sequential)]
    internal struct EVENT_TRACE
    {
        public EVENT_TRACE_HEADER Header;
        public uint InstanceId;
        public uint ParentInstanceId;
        public Guid ParentGuid;
        public IntPtr MofData;
        public uint MofLength;
        public uint ClientContext;  // Reserved.
    }

    //
    // https://docs.microsoft.com/en-us/windows/desktop/etw/trace-logfile-header
    //

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct TRACE_LOGFILE_HEADER
    {
        public uint BufferSize;

        public byte MajorVersion;
        public byte MinorVersion;
        public byte SubVersion;         // Reserved.
        public byte SubMinorVersion;    // Reserved.

        public uint ProviderVersion;
        public uint NumberOfProcessors;
        public long EndTime;
        public uint TimerResolution;
        public uint MaximumFileSize;
        public uint LogFileMode;
        public uint BuffersWritten;

        public uint StartBuffers;       // Reserved.
        public uint PointerSize;
        public uint EventsLost;
        public uint CpuSpeedInMHz;

        public IntPtr LoggerName;       // Do not use.
        public IntPtr LogFileName;      // Do not use.
        public TIME_ZONE_INFORMATION TimeZone;
        public long BootTime;
        public long PerfFreq;
        public long StartTime;
        public uint ReservedFlags;
        public uint BuffersLost;
    }

    //
    // PEVENT_TRACE_BUFFER_CALLBACK callback function
    // https://docs.microsoft.com/en-us/windows/desktop/etw/buffercallback
    //

    internal delegate uint BufferCallback([In] ref EVENT_TRACE_LOGFILE buffer);

    //
    // PEVENT_RECORD_CALLBACK callback function
    // https://docs.microsoft.com/en-us/windows/desktop/etw/eventrecordcallback
    //

    internal delegate void EventRecordCallback([In] ref EVENT_RECORD eventRecord);

    //
    // https://docs.microsoft.com/en-us/windows/desktop/etw/event-trace-logfile
    //

    [StructLayout(LayoutKind.Sequential)]
    internal struct EVENT_TRACE_LOGFILE
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string LogFileName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string LoggerName;

        public ulong CurrentTime;
        public uint BuffersRead;
        public uint ProcessTraceMode;   // This app uses only this field within the union.
        public EVENT_TRACE CurrentEvent;
        public TRACE_LOGFILE_HEADER LogfileHeader;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public BufferCallback BufferCallback;

        public uint BufferSize;
        public uint Filled;
        public uint EventsLost;     // Not used.

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public EventRecordCallback EventCallback;

        [MarshalAs(UnmanagedType.Bool)]
        public bool IsKernelTrace;
        public IntPtr Context;
    }

    //
    // https://docs.microsoft.com/en-us/windows/desktop/api/evntprov/ns-evntprov-_event_descriptor
    //

    [StructLayout(LayoutKind.Sequential)]
    internal struct EVENT_DESCRIPTOR
    {
        public ushort Id;
        public byte Version;
        public byte Channel;
        public byte Level;
        public byte Opcode;
        public ushort Task;
        public ulong Keyword;
    }

    //
    // https://docs.microsoft.com/en-us/windows/desktop/api/evntcons/ns-evntcons-_event_header
    //

    [StructLayout(LayoutKind.Sequential)]
    internal struct EVENT_HEADER
    {
        public ushort Size;
        public ushort HeaderType;
        public ushort Flags;
        public ushort EventProperty;
        public uint ThreadId;
        public uint ProcessId;
        public long TimeStamp;
        public Guid ProviderId;
        public EVENT_DESCRIPTOR EventDescriptor;
        public ulong ProcessorTime;
        public Guid ActivityId;
    }

    //
    // https://docs.microsoft.com/en-us/windows/desktop/api/relogger/ns-relogger-_etw_buffer_context
    //

    [StructLayout(LayoutKind.Sequential)]
    internal struct ETW_BUFFER_CONTEXT
    {
        public byte ProcessorNumber;
        public byte Alignment;
        public ushort LoggerId;
    }

    //
    // https://docs.microsoft.com/en-us/windows/desktop/api/evntcons/ns-evntcons-_event_record
    //

    [StructLayout(LayoutKind.Sequential)]
    internal struct EVENT_RECORD
    {
        public EVENT_HEADER EventHeader;
        public ETW_BUFFER_CONTEXT BufferContext;
        public ushort ExtendedDataCount;
        public ushort UserDataLength;
        public IntPtr ExtendedData;
        public IntPtr UserData;
        public IntPtr UserContext;
    }

    //
    // _EVENT_HEADER_EXTENDED_DATA_ITEM structure
    // https://docs.microsoft.com/en-us/windows/desktop/api/evntcons/ns-evntcons-_event_header_extended_data_item
    //

    [StructLayout(LayoutKind.Explicit)]
    internal struct EVENT_HEADER_EXTENDED_DATA_ITEM
    {
        [FieldOffset(0)]
        public ushort Reserved1;    // Reserved.

        [FieldOffset(2)]
        public ushort ExtType;

        [FieldOffset(4)]
        public ushort Linkage;

        [FieldOffset(6)]
        public ushort DataSize;

        [FieldOffset(8)]
        public ulong DataPtr;
    }

    //
    // The header of NDIS packet fragment record.
    //

    [StructLayout(LayoutKind.Sequential)]
    internal struct NdisEventRecordPacketFragmentHeader
    {
        public uint MiniportIfIndex;
        public uint LowerIfIndex;
        public uint FragmentSize;
    }

    internal static class EventTracingApi
    {
        // ProcessTraceMode in EVENT_TRACE_LOGFILE structure.
        public const uint PROCESS_TRACE_MODE_EVENT_RECORD = 0x10000000;

        // The ETW provider ID of Microsoft-Windows-NDIS-PacketCapture provider.
        public static Guid EtwNdisPacketCaptureProviderId = new Guid("2ED6006E-4729-4609-B423-3EE7BCD678EF");

        // The event record ID of packet fragment by Microsoft-Windows-NDIS-PacketCapture provider.
        public const ushort EtwNdisPacketFragmentRecordId = 1001;

        //
        // OpenTrace function
        // https://docs.microsoft.com/en-us/windows/desktop/etw/opentrace
        //

        [DllImport("sechost.dll", ExactSpelling = true, EntryPoint = "OpenTraceW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern ulong OpenTraceSechost(ref EVENT_TRACE_LOGFILE logfile);

        [DllImport("advapi32.dll", ExactSpelling = true, EntryPoint = "OpenTraceW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern ulong OpenTraceAdvapi32(ref EVENT_TRACE_LOGFILE logfile);

        private delegate ulong OpenTraceNativeApiFunction(ref EVENT_TRACE_LOGFILE logfile);
        private static readonly OpenTraceNativeApiFunction OpenTraceNative;

        //
        // CloseTrace function
        // https://docs.microsoft.com/en-us/windows/desktop/etw/closetrace
        //

        [DllImport("sechost.dll", ExactSpelling = true, EntryPoint = "CloseTrace")]
        private static extern int CloseTraceSechost(ulong traceHandle);

        [DllImport("advapi32.dll", ExactSpelling = true, EntryPoint = "CloseTrace")]
        private static extern int CloseTraceAdvapi32(ulong traceHandle);

        private delegate int CloseTraceNativeApiFunction(ulong traceHandle);
        private static readonly CloseTraceNativeApiFunction CloseTraceNative;

        //
        // ProcessTrace function
        // https://docs.microsoft.com/en-us/windows/desktop/etw/processtrace
        //

        [DllImport("sechost.dll", ExactSpelling = true, EntryPoint = "ProcessTrace")]
        private static extern uint ProcessTraceSechost(ulong[] handleArray, uint handleCount, IntPtr startTime, IntPtr endTime);

        [DllImport("advapi32.dll", ExactSpelling = true, EntryPoint = "ProcessTrace")]
        private static extern uint ProcessTraceAdvapi32(ulong[] handleArray, uint handleCount, IntPtr startTime, IntPtr endTime);

        private delegate uint ProcessTraceNativeApiFunction(ulong[] handleArray, uint handleCount, IntPtr startTime, IntPtr endTime);
        private static readonly ProcessTraceNativeApiFunction ProcessTraceNative;

        static EventTracingApi()
        {
            // Use sechost.dll on Windows 8.1 and Windows Server 2012 R2 and later. Otherwise, use advapi32.dll.
            // Because the DLL is different that exports the event tracing functions.
            var version = Environment.OSVersion.Version;
            var useSechostDll = version.Major > 6 || (version.Major == 6 && version.Minor >= 3);
            if (useSechostDll)
            {
                OpenTraceNative = OpenTraceSechost;
                CloseTraceNative = CloseTraceSechost;
                ProcessTraceNative = ProcessTraceSechost;
            }
            else
            {
                OpenTraceNative = OpenTraceAdvapi32;
                CloseTraceNative = CloseTraceAdvapi32;
                ProcessTraceNative = ProcessTraceAdvapi32;
            }
        }

        public static ulong OpenTrace(ref EVENT_TRACE_LOGFILE logfile)
        {
            return OpenTraceNative(ref logfile);
        }

        public static int CloseTrace(ulong traceHandle)
        {
            return CloseTraceNative(traceHandle);
        }

        public static uint ProcessTrace(ulong[] handleArray, uint handleCount, IntPtr startTime, IntPtr endTime)
        {
            return ProcessTraceNative(handleArray, handleCount, startTime, endTime);
        }

        public static bool IsInvalidProcessTraceHandle(ulong traceHandle)
        {
            // If OpenTrace function fails,
            //     returns 0xFFFFFFFFFFFFFFFF, if application is 64-bit and OS is Vista and later.
            //     returns 0x00000000FFFFFFFF, if application is 32-bit and OS is Vista and later.
            // Ref: https://docs.microsoft.com/en-us/windows/desktop/etw/opentrace
            return (traceHandle == 0) ||
                   (Environment.Is64BitProcess && (traceHandle == 0xFFFFFFFFFFFFFFFF)) ||
                   (!Environment.Is64BitProcess && (traceHandle == 0x00000000FFFFFFFF));
        }
    }
}
