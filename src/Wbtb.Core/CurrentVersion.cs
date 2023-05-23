using System.IO;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    internal class CurrentVersion
    {
        public SemanticVersion CoreVersion { private set; get; }

        public string CurrentHash { private set; get; }

        public void Resolve()
        {
            // read this from currentVersion.txt file in app root
            string currentVersion = "abc123 0.0.0";
            if (File.Exists("./currentVersion.txt"))
              currentVersion = File.ReadAllText("./currentVersion.txt");

            Regex regex = new Regex("^(.*)? (.*)?");
            Match match = regex.Match(currentVersion);
            if (!match.Success)
                throw new ConfigurationException($"currentVersion.txt content {currentVersion} is invalid");

            CurrentHash = match.Groups[1].Value;
            CoreVersion = SemanticVersion.TryParse(match.Groups[2].Value);
        }
    }
}
