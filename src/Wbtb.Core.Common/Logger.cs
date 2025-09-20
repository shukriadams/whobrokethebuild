using System;
using System.Collections.Generic;
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
    /// There
    /// </summary>
    public class Logger
    {
        public bool WriteToConsole { get; set; }

        public bool WriteToDebugConsole { get; set; }

        public bool WriteToFile { get; set; }

        /// <summary>
        /// 0 = most important, higher = less
        /// </summary>
        public int StatusVerbosityThreshold { get; set; }

        /// <summary>
        /// 0 = most important, higher = less
        /// </summary>
        public int DebugVerbosityThreshold { get; set; }

        public IEnumerable<string> DebugSourceFilters { get; set; } = new List<string>();

        public Debug OnDebug { get; set; }

        public Status OnStatus { get; set; }

        public Error OnError { get; set; }

        public Warn OnWarn { get; set; }

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

            string dateString = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : ";

            if (arg == null)
            {
                Console.WriteLine($"ERROR : {dateString}{message}", source);
                System.Diagnostics.Debug.WriteLine($"ERROR : {dateString}{message}", source);
            }
            else 
            {
                Console.WriteLine($"ERROR : {dateString}{message}", arg, source);
                System.Diagnostics.Debug.WriteLine($"ERROR : {dateString}{message}", arg, source);
            }

            if (OnError != null)
                OnError.Invoke(message, arg, source);
        }

        /// <summary>
        /// All bad things that could be recovered from but warrant attention later. Cannot be disabled.
        /// </summary>
        public void Warn(string message, Type sourceType, object arg = null)
        {
            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message.Replace("{", " ")
                    .Replace("}", " ");

            string dateString = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : ";

            if (arg == null)
            {
                Console.WriteLine($"WARN : {dateString}{message}");
                System.Diagnostics.Debug.WriteLine($"WARN : {dateString}{message}");
            }
            else
            {
                Console.WriteLine($"WARN : {dateString}{message}", arg);
                System.Diagnostics.Debug.WriteLine($"WARN : {dateString}{message}", arg);
            }

            if (OnWarn != null)
                OnWarn.Invoke(message, arg);
        }

        /// <summary>
        /// Things that have been done, tells us about normal operation. Verbosity 0 cannot be disabled.
        /// Regular users
        /// </summary>
        public void Status(string message, int verbosity=0)
        {
            if (verbosity > this.StatusVerbosityThreshold)
                return;

            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message.Replace("{", " ")
                    .Replace("}", " ");

            string dateString = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : ";

            Console.WriteLine($"STATUS : {dateString}{message}");
            System.Diagnostics.Debug.WriteLine($"STATUS : {dateString}{message}");

            if (OnStatus != null)
                OnStatus.Invoke(message);
        }

        public void Debug(string message, int verbosity = 0)
        {
            this.Debug(string.Empty, message, verbosity);
        }

        public void Debug(object source, string message, int verbosity = 0) 
        {
            this.Debug(TypeHelper.Name(source), message, verbosity);
        }

        /// <summary>
        /// Things that have been done, tells us about normal operation. Verbosity 0 cannot be disabled.
        /// </summary>
        public void Debug(string map, string message,  int verbosity = 0)
        {
            if (verbosity > this.DebugVerbosityThreshold)
                return;

            if (!string.IsNullOrEmpty(map) && DebugSourceFilters.Any() && !DebugSourceFilters.Any(f => f == map))
                return;

            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message.Replace("{", " ")
                    .Replace("}", " ");

            string dateString = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : ";

            Console.WriteLine($"DEBUG : {dateString}{message}");
            System.Diagnostics.Debug.WriteLine($"DEBUG : {dateString}{message}");

            if (OnDebug != null)
                OnDebug.Invoke(message);
        }

    }
}
