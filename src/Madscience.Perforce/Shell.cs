using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MadScience.Shell
{
    public class ShellResult
    {
        public int ExitCode { get; set; }
        public IEnumerable<string> StdOut { get; set; }
        public IEnumerable<string> StdErr { get; set; }

        public ShellResult()
        { 
            this.StdOut = new string[] { };
            this.StdErr = new string[] { };
        }
    }

    public class Shell
    {
        public static ShellResult RunSh(string command)
        {
            Process cmd = new Process();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                cmd.StartInfo.FileName = "sh";
                cmd.StartInfo.Arguments = $"-c \"{command}\"";
            }
            else
            {
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.Arguments = $"/k {command}";
            }

            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                cmd.WaitForExit();

            List<string> stdOut = new List<string>();
            List<string> stdErr = new List<string>();

            while (!cmd.StandardOutput.EndOfStream)
            {
                string line = cmd.StandardOutput.ReadLine();
                stdOut.Add(line);
                Console.WriteLine(line);
            }

            while (!cmd.StandardError.EndOfStream)
            {
                string line = cmd.StandardError.ReadLine();
                stdErr.Add(line);
                Console.WriteLine(line);
            }

            return new ShellResult
            {
                StdOut = stdOut,
                StdErr = stdErr,
                ExitCode = cmd.ExitCode
            };
        }
    }

}

