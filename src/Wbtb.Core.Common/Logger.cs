using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Wbtb.Core.Common
{
    // We don't want to force a single log file writer on a given app implementing Wbtb, so each app
    // can hook its own file writer up via events

    public delegate void Error(string message, object arg, string source);
    
    public delegate void Warn(string message, object arg);

    public delegate void Status(string message);

    public delegate void Debug(string message);

    /// <summary>
    /// Simplified common logger for everything. Note that while it's best to resolve from di, you can always create a new  instance of this 
    /// in a pinch and it will still at least be reliable than calling Console.WriteLine directly, .net can be such garbage sometimes.
    /// </summary>
    public class Logger
    {
        public bool WriteToConsole { get; set; }

        public bool WriteToDebugConsole { get; set; }

        public bool WriteToFile { get; set; }

        public bool AppendDates { get; set; } = true;
        
        public bool AppendCategory { get; set; } = true;
        
        public bool SendtoDiagnostics { get; set; } = true;

        public bool SendToFile { get; set; } = true;

        /// <summary>
        /// 0 = most important, higher = less
        /// </summary>
        public int StatusVerbosityThreshold { get; set; }

        /// <summary>
        /// 0 = most important, higher = less
        /// </summary>
        public int DebugVerbosityThreshold { get; set; }

        public IEnumerable<string> DebugSourceBlock { get; set; } = new List<string>();

        public IEnumerable<string> DebugSourceAllow { get; set; } = new List<string>();

        public Debug OnDebug { get; set; }

        public Status OnStatus { get; set; }

        public Error OnError { get; set; }

        public Warn OnWarn { get; set; }

        private string GenerateDateString() 
        {
            return this.AppendDates ? $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : " : string.Empty;

        }

        public void Error(object sourceType, Exception arg = null)
        {
            Error(sourceType, string.Empty, arg);
        }

        /// <summary>
        /// All errors that were unexpected or could not be recovered from logged here. Cannot be disabled.
        /// </summary>
        public void Error(object sourceType, string message, Exception arg = null) 
        {
            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message.Replace("{", " ")
                    .Replace("}", " ");

            string source = string.Empty;
            if (sourceType != null)
                source = TypeHelper.Name(sourceType);

            if (!string.IsNullOrEmpty(source))
                source = $"|Source:{source}";

            string dateString = GenerateDateString();
            string category = this.AppendCategory ? "Error : " : string.Empty;

            if (arg == null)
            {
                Console.WriteLine($"{category}{dateString}{message}", source);
                if (this.SendtoDiagnostics)
                    System.Diagnostics.Debug.WriteLine($"{category}{dateString}{message}", source);
            }
            else 
            {
                Console.WriteLine($"{category}{dateString}{message}", arg, source);
                if (this.SendtoDiagnostics)
                    System.Diagnostics.Debug.WriteLine($"{category}{dateString}{message}", arg, source);
            }

            if (this.SendToFile) 
            {
                if (OnError == null)
                    WriteInternal($"{category}{message} {(arg == null ? string.Empty : arg)}{source}");
                else
                    OnError.Invoke(message, arg, source);
            }
        }

        public void Warn(object source, string message, object arg = null) 
        {
            Warn(TypeHelper.Name(source), message, arg);
        }

        public void Warn(Type source, string message, object arg = null)
        {
            Warn(TypeHelper.Name(source), message, arg);
        }

        /// <summary>
        /// All bad things that could be recovered from but warrant attention later. Cannot be disabled.
        /// </summary>
        private void Warn(string source, string message, object arg = null)
        {
            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message.Replace("{", " ")
                    .Replace("}", " ");

            string dateString = GenerateDateString();
            string category = this.AppendCategory ? "WARNING : " : string.Empty;
            if (!string.IsNullOrEmpty(source))
                source = $"|Source:{source}";

            if (arg == null)
            {
                Console.WriteLine($"{category}{dateString}{message}");
                if (this.SendtoDiagnostics)
                    System.Diagnostics.Debug.WriteLine($"{category}{dateString}{message}");
            }
            else
            {
                Console.WriteLine($"{category}{dateString}{message}", arg);
                if (this.SendtoDiagnostics)
                    System.Diagnostics.Debug.WriteLine($"{category}{dateString}{message}", arg);
            }

            if (this.SendToFile)
            {
                if (OnWarn == null)
                    WriteInternal($"{category}{message} {(arg == null ? string.Empty : arg)}{source}");
                else
                    OnWarn.Invoke($"{message}{source}", arg);
            }
        }

        public void Status(string message, int verbosity = 0)
        { 
            Status(string.Empty, message, verbosity);
        }
        public void Status(object source, string message, int verbosity = 0)
        {
            Status(TypeHelper.Name(source), message, verbosity);
        }

        /// <summary>
        /// Things that have been done, tells us about normal operation. Verbosity 0 cannot be disabled.
        /// Regular users
        /// </summary>
        private void Status(string source, string message, int verbosity=0)
        {
            if (verbosity > this.StatusVerbosityThreshold)
                return;

            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message.Replace("{", " ")
                    .Replace("}", " ");

            string dateString = GenerateDateString();
            string category = this.AppendCategory ? "STATUS : " : string.Empty;
            if (!string.IsNullOrEmpty(source))
                source = $"|Source:{source}";

            Console.WriteLine($"{category}{dateString}{message}{source}");
            if (this.SendtoDiagnostics)
                System.Diagnostics.Debug.WriteLine($"{category}{dateString}{message}{source}");

            if (this.SendToFile)
            {
                if (OnStatus == null)
                    WriteInternal($"{category}{message}{source}");
                else
                    OnStatus.Invoke($"{message}{source}");
            }
        }

        public void Debug(object source, string message, int verbosity = 0) 
        {
            this.Debug(TypeHelper.Name(source), message, verbosity);
        }

        /// <summary>
        /// Things that have been done, tells us about normal operation. Verbosity 0 cannot be disabled.
        /// </summary>
        public void Debug(string source, string message,  int verbosity = 0)
        {
            if (verbosity > this.DebugVerbosityThreshold)
                return;

            if (!string.IsNullOrEmpty(source) && DebugSourceAllow.Any() && !DebugSourceAllow.Any(f => f == source))
                return;

            if (!string.IsNullOrEmpty(source) && DebugSourceBlock.Any(f => f == source))
                return;

            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message.Replace("{", " ")
                    .Replace("}", " ");

            string dateString = GenerateDateString();
            string category = this.AppendCategory ? "DEBUG : " : string.Empty;
            if (!string.IsNullOrEmpty(source))
                source = $"|Source:{source}";

            Console.WriteLine($"{category}{dateString}{message}{source}");
            if (this.SendtoDiagnostics)
                System.Diagnostics.Debug.WriteLine($"{category}{dateString}{message}{source}");

            if (this.SendToFile)
            {
                if (OnDebug == null)
                    WriteInternal($"{category}{message}{source}");
                else
                    // need to send debug to seri because microsoft . -_-
                    OnDebug.Invoke($"{category}{message}{source}"); 
            }
        }

        /// <summary>
        /// Rudimentary fallback filewrite for logger
        /// </summary>
        /// <param name="text"></param>
        private static void WriteInternal(string text)
        {
            DirectoryInfo currentPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            string logPath = Path.Combine(currentPath.FullName, $"WBTB-fallback-log_{DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.txt");
            string date = DateTime.UtcNow.ToString("yyyy-MM-dd THH\\:mm\\:ss", CultureInfo.InvariantCulture);
            try
            {
                File.AppendAllText(logPath, $"{date} : {text}\n");
            }
            catch (Exception)
            {
                // ignore errors
            }
        }
    }
}
