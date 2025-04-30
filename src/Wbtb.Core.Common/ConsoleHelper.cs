using System;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// WBTB-formatted console methods. 
    /// </summary>
    public class ConsoleHelper
    {
        public static void WriteLine(object arg, bool addDate = true)
        {
            if (addDate)
            {
                Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} :", arg);
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} :", arg);
            }
            else 
            {
                Console.WriteLine(arg);
                System.Diagnostics.Debug.WriteLine(arg);
            }
        }

        /// <summary>
        /// Write text + date info. optional console.writeline replacement
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public static void WriteLine(string message, object arg = null, bool addDate = true)
        {
            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message.Replace("{", " ")
                    .Replace("}", " ");

            string dateString = string.Empty;
            if (addDate)
                dateString = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : ";

            Console.WriteLine($"{dateString}{message}", arg);
            System.Diagnostics.Debug.WriteLine($"{dateString}{message}", arg);
        }

        /// <summary>
        /// Write text + date info. optional console.writeline replacement. Posts type name of posting object
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public static void WriteLine(object sourceObject, string message, object arg = null, bool addDate = true)
        {
            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message.Replace("{", " ")
                    .Replace("}", " ");

            string dateString = string.Empty;
            if (addDate)
                dateString = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : ";

            Console.WriteLine($"{dateString}{sourceObject.GetType().Name} : {message}", arg);
            System.Diagnostics.Debug.WriteLine($"{dateString}{sourceObject.GetType().Name} : {message}", arg);
        }
    }
}
