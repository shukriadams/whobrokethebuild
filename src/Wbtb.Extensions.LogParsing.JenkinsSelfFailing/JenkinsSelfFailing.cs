using System;
using System.Linq;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.JenkinsSelfFailing
{
    public class JenkinsSelfFailing : Plugin, ILogParserPlugin
    {
        private readonly string InternalErrorRegex = @"\b(at org.jenkinsci.|at jenkins.|at hudson.)\b.*";

        private readonly string TimeoutRegex = @"Build timed out \(after \d+ minutes\). Marking the build as failed.";

        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult{ 
                Success = true, 
                SessionId = Guid.NewGuid().ToString()
            };
        }

        string ILogParserPlugin.Parse(Build build, string raw)
        {
            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            Cache cache = di.Resolve<Cache>();

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            // internal error - try for cache
            string internalErrorHash = Sha256.FromString(InternalErrorRegex + fullErrorLog);
            CachePayload internaleErrorCacheLookup = cache.Get(this,job, build, internalErrorHash);
            if (internaleErrorCacheLookup.Payload != null)
                return internaleErrorCacheLookup.Payload;

            // look for following matches
            // at org.jenkinsci
            // at jenkins.
            // at hudson.
            // at java
            MatchCollection matches = new Regex(InternalErrorRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(fullErrorLog);
            string result = string.Empty;
            if (matches.Any()) 
            {
                BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                foreach(Match match in matches) 
                {
                    builder.AddItem(match.Value, "internalError");
                    builder.NewLine();
                }

                result =  builder.GetText();
                cache.Write(this, job, build, internalErrorHash, result);
                return result;
            }


            // timeout error
            string timeoutErrorHash = Sha256.FromString(TimeoutRegex + fullErrorLog);
            CachePayload timeoutLookup = cache.Get(this, job, build, timeoutErrorHash);
            if (timeoutLookup.Payload != null)
                return timeoutLookup.Payload;

            matches = new Regex(TimeoutRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(fullErrorLog);
            result = string.Empty;
            if (matches.Any())
            {
                BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                foreach (Match match in matches)
                {
                    builder.AddItem(match.Value, "buildTimeout");
                    builder.NewLine();
                }

                result = builder.GetText();
                cache.Write(this, job, build, internalErrorHash, result);
            }

            return result;
        }
    }
}
