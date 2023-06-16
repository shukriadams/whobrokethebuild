using System;
using System.IO;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    /// <summary>
    /// Fetches WBTB config froma git repo
    /// </summary>
    public class ConfigurationBootstrapper
    {
        /// <summary>
        /// Ensures latest config. returns true of config has changed
        /// </summary>
        /// <returns></returns>
        public bool EnsureLatest() 
        {
            if (Environment.GetEnvironmentVariable("WBTB_GIT_CONFIG_SYNC") != "true")
                return false;

            string gitRemote = Environment.GetEnvironmentVariable("WBTB_GIT_CONFIG_REPO_URL");
            string localgiturlfile = "./.giturl";
            if (File.Exists(localgiturlfile))
                gitRemote = File.ReadAllText(localgiturlfile);

            if (string.IsNullOrEmpty(gitRemote))
                throw new Exception("WBTB_GIT_CONFIG_REPO_URL required");

            string localPath = Environment.GetEnvironmentVariable("WBTB_GIT_CONFIG_LOCAL_PATH");
            if (string.IsNullOrEmpty(localPath))
                throw new Exception("WBTB_GIT_CONFIG_LOCAL_PATH required");

            string cachePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "ConfigCache");
            string checkoutPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "ConfigCheckout");

            Directory.CreateDirectory(cachePath);

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

            shell = new Shell();
            shell.Run("git rev-parse HEAD");

            string lastChangeHash = string.Empty;
            string currentHashCachePath = Path.Join(cachePath, "currenthash");
            if (File.Exists(currentHashCachePath))
                lastChangeHash = File.ReadAllText(currentHashCachePath);

            string configFileLocalPath = Path.Join(checkoutPath, localPath);

            if (!File.Exists(configFileLocalPath))
                throw new ConfigurationException($"Expected config file {localPath} was not found in checkout from remote repo");

            string configFileContent = File.ReadAllText(configFileLocalPath);
            string configFileHash = Sha256.FromString(configFileContent);

            File.Copy(configFileLocalPath, Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.yml"), true);

            if (lastChangeHash != configFileHash)
            {
                File.WriteAllText(currentHashCachePath, configFileHash);
                Console.WriteLine($"CONFIG HAS CHANGED ({configFileHash}), exiting automatically. Restart app if required.");
                return true;
            }

            return false;
        }
    }
}
