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
        }

        /// <summary>
        /// Write text + date info. optional console.writeline replacement
        /// </summary>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public static void WriteLine(string message, object arg = null)
        {
            Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : {message}" , arg);
        }

        /// <summary>
        /// Write text + date info. optional console.writeline replacement. Posts type name of posting object
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <param name="message"></param>
        /// <param name="arg"></param>
        public static void WriteLine(object sourceObject, string message, object arg = null)
        {
            Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} : {sourceObject.GetType().Name} : {message}", arg);
        }
    }
}
