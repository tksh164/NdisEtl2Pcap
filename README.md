# NdisEtl2Pcap

NdisEtl2Pcap converts from the network trace file that creates by the netsh command to the pcap file.

## Usage

```
NdisEtl2Pcap.exe <Input ETL File Path> <Output PCAP File Path>
```

Example:

```
>NdisEtl2Pcap.exe netsh-trace.etl netsh-trace.pcap
TotalEventRecordCount: 616557
TotalNdisEventRecordCount: 616460
OldestNdisEventRecordTimestamp: 4/11/2018 10:30:40 AM
NewestNdisEventRecordTimestamp: 4/11/2018 12:30:39 PM
Elapsed: 00:00:04.8648188
```

## Capture network trace using the netsh command

You can create a network trace etl file using the netsh trace command. From Windows 7/Windows Server 2008 R2, the netsh command has trace sub-command. [Details are here](https://docs.microsoft.com/en-us/windows/desktop/ndf/network-tracing-in-windows-7).

Example:

```
>netsh trace start capture=yes report=disabled correlation=disabled maxSize=500 traceFile="C:\temp\nettrace.etl"

Trace configuration:
-------------------------------------------------------------------
Status:             Running
Trace File:         C:\temp\nettrace.etl
Append:             Off
Circular:           On
Max Size:           500 MB
Report:             Disabled

>netsh trace stop
Merging traces ... done
File location = C:\temp\nettrace.etl
Tracing session was successfully stopped.
```

## Related

- [CaptureNetworkTraceByPowerShell](https://github.com/tksh164/CaptureNetworkTraceByPowerShell): Network trace capturing script by PowerShell with netsh command.
