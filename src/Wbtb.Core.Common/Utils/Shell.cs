﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Wbtb.Core.Common.Utils
{
    public class Shell
    {
        #region FIELDS

        public event SimpleEvent OnStdOut;

        public event SimpleEvent OnStdErr;

        public IList<string> StdOut = new List<string>();

        public IList<string> StdErr = new List<string>();

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
            command.StartInfo.FileName = "cmd.exe";
            command.StartInfo.Arguments = $"/k {cmd}";
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