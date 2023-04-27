using System;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Extensions.LogParsing.JenkinsSelfFailing
{
    public class JenkinsSelfFailing : Plugin, ILogParser
    {
        public PluginInitResult InitializePlugin()
        {
            return new PluginInitResult{ 
                Success = true, 
                SessionId = Guid.NewGuid().ToString()
            };
        }

        public string Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;
            
            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            // look for following matches
            // at org.jenkinsci
            // at jenkins.
            // at hudson.
            // at java

            MatchCollection matches = new Regex(@"\b(at org.jenkinsci.|at jenkins.|at hudson.)\b.*", RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(fullErrorLog);
            if (matches.Count > 0)
                return $"Build failuire is likely an internal Jenkins error : \r\r{string.Join(string.Empty, matches)}";

            return null;
        }
    }
}
