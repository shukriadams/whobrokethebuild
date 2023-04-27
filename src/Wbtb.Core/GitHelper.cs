using System;
using System.IO;
using Wbtb.Core.Common.Utils;

namespace Wbtb.Core
{
    public class GitHelper
    {
        private static void EnsureLatest(string gitRemote, string checkoutPath)
        {
            // ensure git is available
            Shell shell = new Shell();
            shell.Run("git");
            if (shell.StdErr.Count > 0)
                throw new Exception("git not installed");

            if (Directory.Exists(checkoutPath))
            {
                // git update
                shell = new Shell();
                shell.WorkingDirectory = checkoutPath;
                shell.Run("git reset --hard");
                shell.Run("git clean -dfx");
                shell.Run("git pull");
            }
            else
            {
                // git clone
                shell = new Shell();
                shell.Run($"git clone {gitRemote} {checkoutPath}");
            }
        }

        public static string GetLatestTag(string gitRemote, string checkoutPath)
        { 
            EnsureLatest(gitRemote, checkoutPath);
            Shell shell = new Shell();
            shell.WorkingDirectory = checkoutPath;
            shell.Run("git describe --abbrev=0 --tags");
            if (shell.StdOut.Count > 0)
                return shell.StdOut[0];

            return null;
        }
    }
}
