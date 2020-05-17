using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace SharedStopEnabler.Util
{
    public static class Log
    {
        private static readonly object LogLock = new object();

        private static readonly string LogFilename = Path.Combine(Application.dataPath, "SSE.txt");

        private enum LogType
        {
            Debug,
            Info,
            Warning,
            Error
        }

        private static Stopwatch sw = Stopwatch.StartNew();

        static Log()
        {
            try
            {
                if (File.Exists(LogFilename))
                {
                    File.Delete(LogFilename);
                }
            }
            catch (Exception) { }
        }

        [Conditional("DEBUG")]
        public static void Debug(string s)
        {
            WriteToFile(s, LogType.Debug);
            UnityEngine.Debug.Log(s);
        }

        public static void Info(string s)
        {
            WriteToFile(s, LogType.Info);
        }

        public static void Warning(string s)
        {
            WriteToFile(s, LogType.Warning);
        }

        public static void Error(string s)
        {
            WriteToFile(s, LogType.Error);
        }

        private static void WriteToFile(string log, LogType type)
        {
            lock (LogLock)
            {
                using (StreamWriter w = File.AppendText(LogFilename))
                {
                    long secs = sw.ElapsedTicks / Stopwatch.Frequency;
                    long fraction = sw.ElapsedTicks % Stopwatch.Frequency;
                    w.WriteLine($"{type} {secs:n0}.{fraction:D7}: {log}");

                    if (type == LogType.Warning || type == LogType.Error)
                    {
                        w.WriteLine((new StackTrace()).ToString());
                        w.WriteLine();
                    }
                }
            }
        }
    }
}
