using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.VSPackage1
{
    public static class Configuration
    {
        private const string RegistryKey = @"Software\nProg\VSPackage1";

        #region Registry Functions
        private static Dictionary<string, object> _cache = new Dictionary<string, object>();

        private static int ReadInteger(string name, int defaultValue = 0)
        {
            object cachedValue;
            if (_cache.TryGetValue(name, out cachedValue) &&
                cachedValue is int)
                return (int)cachedValue;

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryKey))
                {
                    var val = key.GetValue(name);
                    if (val != null &&
                        val is int)
                        return (int)val;
                }
            }
            catch { }

            return defaultValue;
        }

        private static void WriteInteger(string name, int value)
        {
            _cache[name] = value;
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryKey))
                key.SetValue(name, value, RegistryValueKind.DWord);
        }

        private static string ReadString(string name, string defaultValue = null)
        {
            object cachedValue;
            if (_cache.TryGetValue(name, out cachedValue) &&
                cachedValue is string)
                return (string)cachedValue;

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryKey))
                {
                    var val = key.GetValue(name);
                    if (val != null &&
                        val is string)
                        return (string)val;
                }
            }
            catch { }

            return defaultValue;
        }

        private static void WriteString(string name, string value)
        {
            _cache[name] = value;
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryKey))
                key.SetValue(name, value, RegistryValueKind.String);
        }
        #endregion

        public static bool EnableDebugLog
        {
            get
            {
                return ReadInteger("EnableDebugLog", 0) != 0;
            }
            set
            {
                WriteInteger("EnableDebugLog", value ? 1 : 0);
            }
        }

        public static string DebugLogFilename
        {
            get
            {
                var filename = ReadString("DebugLogFilename");
                if (filename == null)
                {
                    DebugLogFilename = filename = Path.Combine(Path.GetTempPath(), "vspackage1_log.txt");
                    EnableDebugLog = EnableDebugLog;
                }

                return filename;
            }
            set
            {
                WriteString("DebugLogFilename", value);
            }
        }
    }
}
