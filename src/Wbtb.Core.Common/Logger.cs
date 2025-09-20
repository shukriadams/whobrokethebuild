using System;
using System.Collections.Generic;

namespace Wbtb.Core.Common
{
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

        public IEnumerable<string> DebugVerbosityAllow { get; set; } = new List<string>();

        /// <summary>
        /// All errors that were unexpected or could not be recovered from logged here. Cannot be disabled.
        /// </summary>
        public void Error(string message, object arg = null) 
        {
            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message.Replace("{", " ")
                    .Replace("}", " ");

            string dateString = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : ";

            if (arg == null)
            {
                Console.WriteLine($"ERROR : {dateString}{message}");
                System.Diagnostics.Debug.WriteLine($"ERROR : {dateString}{message}");
            }
            else 
            {
                Console.WriteLine($"ERROR : {dateString}{message}", arg);
                System.Diagnostics.Debug.WriteLine($"ERROR : {dateString}{message}", arg);
            }
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
        }

        /// <summary>
        /// Things that have been done, tells us about normal operation. Verbosity 0 cannot be disabled.
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
        }

        /// <summary>
        /// Things that have been done, tells us about normal operation. Verbosity 0 cannot be disabled.
        /// </summary>
        public void Debug(string message, string map, int verbosity = 0)
        {
            if (verbosity > this.DebugVerbosityThreshold)
                return;

            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message.Replace("{", " ")
                    .Replace("}", " ");

            string dateString = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : ";

            Console.WriteLine($"DEBUG : {dateString}{message}");
            System.Diagnostics.Debug.WriteLine($"DEBUG : {dateString}{message}");
        }

    }
}
