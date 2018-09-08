using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using NdisEtl2PcapLib;
using NdisEtl2PcapLib.Pcap;

namespace NdisEtl2Pcap
{
    internal class Program
    {
        private static ResultSummary ResultSummary { get; set; }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionTrapper.UnhandledExceptionTrapper);

            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }
            var etlFilePath = args[0];
            var pcapFilePath = args[1];

            ResultSummary = new ResultSummary();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var packets = LoadNdisRecords(etlFilePath);
            PcapFile.WritePcapFile(pcapFilePath, packets);

            stopwatch.Stop();
            ResultSummary.Elapsed = stopwatch.Elapsed;

            PrintResultSummary();
        }

        private static void PrintUsage()
        {
            var exeName = Path.GetFileName(Assembly.GetEntryAssembly().Location);
            Console.WriteLine("Usage: {0} <Input ETL File Path> <Output PCAP File Path>", exeName);
        }

        private static NdisEventRecord[] LoadNdisRecords(string etlFilePath)
        {
            NdisEventRecord[] records;
            using (var ndisEtlFile = NdisEtlFile.Load(etlFilePath))
            {
                records = ndisEtlFile.Records;

                ResultSummary.TotalEventRecordCount = ndisEtlFile.TotalEventRecordCount;
                ResultSummary.TotalNdisEventRecordCount = ndisEtlFile.TotalNdisEventRecordCount;
                ResultSummary.OldestNdisEventRecordTimestamp = ndisEtlFile.OldestNdisEventRecordTimestamp;
                ResultSummary.NewestNdisEventRecordTimestamp = ndisEtlFile.NewestNdisEventRecordTimestamp;
            }
            return records;
        }

        private static void PrintResultSummary()
        {
            Console.WriteLine("TotalEventRecordCount: {0}", ResultSummary.TotalEventRecordCount);
            Console.WriteLine("TotalNdisEventRecordCount: {0}", ResultSummary.TotalNdisEventRecordCount);
            Console.WriteLine("OldestNdisEventRecordTimestamp: {0}", ResultSummary.OldestNdisEventRecordTimestamp);
            Console.WriteLine("NewestNdisEventRecordTimestamp: {0}", ResultSummary.NewestNdisEventRecordTimestamp);
            Console.WriteLine("Elapsed: {0}", ResultSummary.Elapsed);
        }
    }

    internal class ResultSummary
    {
        public long TotalEventRecordCount { get; set; }
        public long TotalNdisEventRecordCount { get; set; }
        public DateTime OldestNdisEventRecordTimestamp { get; set; }
        public DateTime NewestNdisEventRecordTimestamp { get; set; }
        public TimeSpan Elapsed { get; set; }
    }
}
