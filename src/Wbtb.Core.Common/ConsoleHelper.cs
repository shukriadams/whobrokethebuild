using System;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// WBTB-formatted console methods. 
    /// </summary>
    public class ConsoleHelper
    {
        public static void WriteLine(object arg)
        {
            Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} :", arg);
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} :", arg);
        }

        /// <summary>
        /// Write text + date info. optional console.writeline replacement
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public static void WriteLine(string message, object arg = null)
        {
            // strip out curly braces from messages, these will break console out on C#
            message = message.Replace("{", " ")
                .Replace("}", " ");

            Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : {message}", arg);
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : {message}", arg);
        }

        /// <summary>
        /// Write text + date info. optional console.writeline replacement. Posts type name of posting object
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public static void WriteLine(object sourceObject, string message, object arg = null)
        {
            // strip out curly braces from messages, these will break console out on C#
            message = message.Replace("{", " ")
                .Replace("}", " ");

            Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : {sourceObject.GetType().Name} : {message}", arg);
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : {sourceObject.GetType().Name} : {message}", arg);
        }
    }
}
