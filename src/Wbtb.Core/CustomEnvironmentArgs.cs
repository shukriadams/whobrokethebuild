using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Wbtb.Core
{
    public class CustomEnvironmentArgs
    {
        /// <summary>
        /// Applies name:value arg in .env file in project root. this is a dev aid.
        /// </summary>
        public static void Apply()
        {
            string envArgFilePath = null;
            DirectoryInfo currentPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            
            while (currentPath.Parent != null) 
            { 
                if (File.Exists(Path.Combine(currentPath.FullName, ".env"))) 
                {
                    envArgFilePath = Path.Combine(currentPath.FullName, ".env");
                    break;
                }
                
                currentPath = currentPath.Parent;
            }

            if (envArgFilePath == null)
                return;

            Console.WriteLine("custom env variable file found, applying");

            string filecontent = File.ReadAllText(envArgFilePath);
            filecontent = filecontent.Replace("\r\n", "\n");
            string[] args = filecontent.Split("\n");
            Regex r = new Regex(@"(.*)?=(.*)");

            foreach(string arg in args)
            {
                Match match = r.Match(arg);
                if (!match.Success)
                    continue;

                if (match.Groups.Count < 3)
                    continue;

                Environment.SetEnvironmentVariable(match.Groups[1].Value, match.Groups[2].Value);
                Console.WriteLine($"set environment variable {match.Groups[1].Value}");
            }
                
        }
    }
}
