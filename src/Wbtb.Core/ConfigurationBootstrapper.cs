using System;
using System.IO;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    /// <summary>
    /// Fetches WBTB config from a git repo
    /// </summary>
    public class ConfigurationBootstrapper
    {
        /// <summary>
        /// Ensures latest config. Returns true of config has changed
        /// </summary>
        /// <returns></returns>
        public bool EnsureLatest(bool verbose) 
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WBTB_GIT_CONFIG_REPO_URL")))
            {
                if (verbose)
                    ConsoleHelper.WriteLine("GIT-CONFIG : sync disabled, skipping");
                return false;
            }

            if (verbose)
                ConsoleHelper.WriteLine("GIT-CONFIG : sync enabled");

            string gitRemote = Environment.GetEnvironmentVariable("WBTB_GIT_CONFIG_REPO_URL");
            string localgiturlfile = "./.giturl";
            if (File.Exists(localgiturlfile)) 
            {
                gitRemote = File.ReadAllText(localgiturlfile);
                if (string.IsNullOrEmpty(gitRemote))
                    throw new Exception("GIT-CONFIG : ./.giturl override exists, but is empty. File should contain git url to sync config from.");
            }

            string localPath = "config.yml";

            ConfigurationBasic configurationBasic = new ConfigurationBasic();
            string cachePath = Path.Join(configurationBasic.DataRootPath, "ConfigCache");
            string checkoutPath = Path.Join(configurationBasic.DataRootPath, "ConfigCheckout");

            Directory.CreateDirectory(cachePath);

            // ensure git is available
            Shell shell = new Shell();
            shell.Run("git");
            if (shell.StdErr.Count > 0)
                throw new Exception("GIT-CONFIG : git not found, but required to sync config. Please install git and restart server.");

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
                string result = shell.Run($"git clone {gitRemote} {checkoutPath}");
                ConsoleHelper.WriteLine($"GIT-CONFIG : {result}");
            }

            shell = new Shell();
            shell.Run("git rev-parse HEAD");

            string configFileLocalPath = Path.Join(checkoutPath, localPath);

            if (!File.Exists(configFileLocalPath))
                throw new ConfigurationException($"GIT-CONFIG : Expected config file {localPath} was not found at checkout path {configFileLocalPath}. Did git clone fail?");

            string targetConfigFilePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.yml");
            string targetConfigFileHash = string.Empty;
            if (File.Exists(targetConfigFilePath)) 
                targetConfigFileHash = Sha256.FromString(File.ReadAllText(targetConfigFilePath));

            string incomingConfigFileHash = Sha256.FromString(File.ReadAllText(configFileLocalPath));

            if (targetConfigFileHash != incomingConfigFileHash)
            {
                File.Copy(configFileLocalPath, Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.yml"), true);
                shell = new Shell();
                string commitMessage = shell.Run($"git log --format=%B -n 1 {incomingConfigFileHash}");
                ConsoleHelper.WriteLine($"GIT-CONFIG : config has changed. Config hash was {targetConfigFileHash}, is now {incomingConfigFileHash} ({commitMessage}).");
                return true;
            }

            if (verbose)
                ConsoleHelper.WriteLine($"GIT-CONFIG : Config unchanged at hash {targetConfigFileHash}.");
            return false;
        }
    }
}
