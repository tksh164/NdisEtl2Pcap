using System;
using System.Text;

namespace NdisEtl2Pcap
{
    internal static class ExceptionTrapper
    {
        public static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            var exceptionInfoText = BuildExceptionInformationText(ex);
            Console.Error.WriteLine("**** EXCEPTION ****");
            Console.Error.WriteLine(exceptionInfoText);
        }

        public static string BuildExceptionInformationText(Exception exception)
        {
            var builder = new StringBuilder();

            var ex = exception;
            while (true)
            {
                builder.AppendLine(string.Format("{0}: {1}", ex.GetType().FullName, ex.Message));

                // Special handling for each exception type.
                switch (ex)
                {
                    case System.ComponentModel.Win32Exception win32Exception:
                        builder.AppendLine(string.Format("NativeErrorCode: {0}", win32Exception.NativeErrorCode));
                        builder.AppendLine(string.Format("ErrorCode: 0x{0:x8}", win32Exception.ErrorCode));
                        builder.AppendLine(string.Format("HResult: 0x{0:x8}", win32Exception.HResult));
                        break;
                }

                if (ex.Data.Count != 0)
                {
                    builder.AppendLine("Data:");
                    foreach (string key in ex.Data.Keys)
                    {
                        builder.AppendLine(string.Format("   {0}: {1}", key, ex.Data[key]));
                    }
                }

                builder.AppendLine("Stack Trace:");
                builder.AppendLine(ex.StackTrace);

                if (ex.InnerException == null) break;

                ex = ex.InnerException;
                builder.AppendLine(@"--- Inner exception is below ---");
            }

            return builder.ToString();
        }
    }
}
