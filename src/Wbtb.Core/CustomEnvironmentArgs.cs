using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Wbtb.Core
{
    /// <summary>
    /// Loads env vars from .env file. Intended for development environments only, but can in theory be used elsewhere.
    /// Env file should contain name=value, one per line.
    /// </summary>
    public class CustomEnvironmentArgs
    {
        /// <summary>
        /// Applies name:value arg in .env file in project root. this is a dev aid.
        /// </summary>
        public void Apply(bool verbose)
        {
            string envArgFilePath = null;
    
            // Crawl up directory tree from app start dir, look for .env file until reach disk root.
            // Necessary because basedirectory varies between web and CLI app.
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

            string fileContent = File.ReadAllText(envArgFilePath);
            fileContent = fileContent.Replace("\r\n", "\n");
            string[] args = fileContent.Split("\n");
            Regex envVarRegex = new Regex(@"(.*)?=(.*)");

            foreach(string arg in args)
            {
                Match match = envVarRegex.Match(arg);
                if (!match.Success)
                    continue;

                if (match.Groups.Count < 3)
                    continue;

                Environment.SetEnvironmentVariable(match.Groups[1].Value, match.Groups[2].Value);
                if (verbose)
                    Console.WriteLine($"WBTB : Set environment variable {match.Groups[1].Value}");
            }
                
        }
    }
}
    