using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Company.VSPackage1
{
    public static class Logger
    {
        private static long _count = 0;

        static Logger()
        {
            Clear();
            Log(LogType.Debug, "Started instance");
        }

        public static void Log(LogType type, string text)
        {
            if (!Configuration.EnableDebugLog &&
                type == LogType.Debug)
                return;

            var sb = new StringBuilder(256);

            if (Interlocked.Increment(ref _count) > 1)
                sb.Append("\r\n");

            var date = DateTime.Now;
            sb.AppendFormat("[{0:00}:{1:00}:{2:00}][{3}] ", date.Hour, date.Minute, date.Second, Enum.GetName(typeof(LogType), type));
            sb.Append(text);

            File.AppendAllText(Configuration.DebugLogFilename, sb.ToString());
        }

        public static void Log(LogType type, string format, params object[] args)
        {
            Log(type, string.Format(format, args));
        }

        public static void Clear()
        {
            File.WriteAllText(Configuration.DebugLogFilename, "");
        }
    }

    public enum LogType : byte
    {
        Normal = 0,
        Warning = 1,
        Error = 2,
        Debug = 3,
        Security = 4
    }
}
