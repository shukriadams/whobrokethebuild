using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public class SemanticVersion
    {
        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Patch { get; private set; }

        public string Extra { get; private set; }

        public override string ToString()
        {
            return $"{this.Major}.{this.Minor}.{this.Patch}{this.Extra}";
        }

        public SemanticVersion(int major, int minor, int patch, string extra = "") 
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.Extra = extra; 
        }

        public static SemanticVersion TryParse(string input) 
        {
            Regex regex = new Regex(@"^(\d+?)\.(\d+?)\.(\d+?)(.*)?$");
            Match match = regex.Match(input);
            if (match == null || !match.Success)
                throw new ConfigurationException($"{input} could not parsed into a semantic version tag");
            
            SemanticVersion version = new SemanticVersion(
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value),
                match.Groups.Count > 3 ? match.Groups[4].Value : "");

            return version;
        }

    }
}
