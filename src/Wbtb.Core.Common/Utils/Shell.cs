using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Wbtb.Core.Common
{
    public class Shell
    {
        #region FIELDS

        public event SimpleEvent OnStdOut;

        public event SimpleEvent OnStdErr;

        public IList<string> StdOut = new List<string>();

        public IList<string> StdErr = new List<string>();

        private string _mode = "sh";

        #endregion

        #region PROPERTIES

        public bool WriteToConsole { get; set; }

        public string WorkingDirectory { get;set;}

        #endregion

        #region CTORS

        public Shell()
        {
            this.WorkingDirectory = string.Empty;
        }

        #endregion

        #region METHODS

        public string Run(string cmd)
        {
            Process command = new Process();

            // for now we're forcing sh as a standard shell, if you're running windows and don't have sh at the cmd line install git-for-windows and map its sh.exe to your path
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                command.StartInfo.FileName = "sh";
                command.StartInfo.Arguments = $"-c \"{cmd}\"";
            }
            else
            {
                command.StartInfo.FileName = "cmd.exe";
                command.StartInfo.Arguments = $"/k {cmd}";
            }

            command.StartInfo.WorkingDirectory = this.WorkingDirectory;
            command.StartInfo.RedirectStandardInput = true;
            command.StartInfo.RedirectStandardOutput = true;
            command.StartInfo.RedirectStandardError = true;
            command.StartInfo.CreateNoWindow = true;
            command.StartInfo.UseShellExecute = false;
            command.OutputDataReceived += Cmd_OnStdOut;
            command.ErrorDataReceived += Cmd_OnStdError;

            command.Start();

            // std in must be flushed and closed
            command.StandardInput.Flush();
            command.StandardInput.Close();
            command.BeginOutputReadLine();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                command.WaitForExit();

            return string.Join(string.Empty, StdOut);
        }

        #endregion

        #region EVENTS

        private void Cmd_OnStdError(object sender, DataReceivedEventArgs e)
        {
            OnStdErr?.Invoke(e.Data);

            this.StdErr.Add(e.Data);

            if (this.WriteToConsole)
                Console.WriteLine(e.Data);
        }

        private void Cmd_OnStdOut(object sender, DataReceivedEventArgs e)
        {
            OnStdOut?.Invoke(e.Data);

            this.StdOut.Add(e.Data);

            if (this.WriteToConsole)
                Console.WriteLine(e.Data);
        }

        #endregion
    }
}