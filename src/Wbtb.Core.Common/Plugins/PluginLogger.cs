using System;
using System.Globalization;
using System.IO;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Simple common file logger for plugins, no dependencies 
    /// </summary>
    public class PluginLogger
    {
        public static void WriteError(string text, Exception ex)
        {
            WriteInternal($"{text} {ex}");
        }

        public static void Write(string text)
        {
            WriteInternal($"{text}");
        }

        private static void WriteInternal(string text)
        {
            Console.WriteLine(text);

            DirectoryInfo currentPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            string logPath = Path.Combine(currentPath.FullName, $"{DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.txt");
            string date = DateTime.UtcNow.ToString("yyyy-MM-dd THH\\:mm\\:ss", CultureInfo.InvariantCulture);
            try 
            {
                File.AppendAllText(logPath, $"{date} : {text}\n");
            }
            catch(Exception)
            {
                // ignore errors
            }
        }
    }
}
