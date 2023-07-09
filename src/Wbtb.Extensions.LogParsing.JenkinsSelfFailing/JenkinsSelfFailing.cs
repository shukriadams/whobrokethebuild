using System;
using System.Linq;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.JenkinsSelfFailing
{
    public class JenkinsSelfFailing : Plugin, ILogParserPlugin
    {
        private readonly string RegexPattern = @"\b(at org.jenkinsci.|at jenkins.|at hudson.)\b.*";

        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult{ 
                Success = true, 
                SessionId = Guid.NewGuid().ToString()
            };
        }
        
        string ILogParserPlugin.ParseAndCache(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            SimpleDI di = new SimpleDI();

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            // try for cache
            string hash = Sha256.FromString(RegexPattern + fullErrorLog);
            Cache cache = di.Resolve<Cache>();
            string resultLookup = cache.Get(this, hash);
            if (resultLookup != null)
                return resultLookup;


            resultLookup = ((ILogParserPlugin)this).Parse(raw);
            cache.Write(this, hash, resultLookup);
            return resultLookup;
        }

        string ILogParserPlugin.Parse(string raw)
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

            MatchCollection matches = new Regex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(fullErrorLog);
            if (matches.Any()) 
            {
                BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                foreach(Match match in matches) 
                {
                    builder.AddItem(match.Value);
                    builder.NewLine();
                }

                return builder.GetText();
            }

            return null;
        }
    }
}
