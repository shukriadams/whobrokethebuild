using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            int maxLineLength = 0;
            if (ContextPluginConfig.Config.Any(r => r.Key == "MaxLineLength"))
                int.TryParse(ContextPluginConfig.Config.First(r => r.Key == "MaxLineLength").Value.ToString(), out maxLineLength);

            string chunkDelimiter = string.Empty;
            if (ContextPluginConfig.Config.Any(r => r.Key == "SectionDelimiter"))
                chunkDelimiter = ContextPluginConfig.Config.First(r => r.Key == "SectionDelimiter").Value.ToString();

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            Cache cache = di.Resolve<Cache>();


            // internal error - try for cache
            string internalErrorHash = Sha256.FromString(InternalErrorRegex + TimeoutRegex + raw);
            CachePayload internaleErrorCacheLookup = cache.Get(this,job, build, internalErrorHash);
            if (internaleErrorCacheLookup.Payload != null)
                return internaleErrorCacheLookup.Payload;


            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            StringBuilder result = new StringBuilder();
            IEnumerable<string> chunks = null;
            if (string.IsNullOrEmpty(chunkDelimiter))
                chunks = new List<string> { fullErrorLog };
            else
                chunks = fullErrorLog.Split(chunkDelimiter);

            foreach (string chunk in chunks)
            {
                // filter out chunks that can poison regex with overly-long sequences
                if (maxLineLength > 0)
                {
                    int maxContinuousLineLengthInChunk = chunk.Split(" ").OrderByDescending(r => r.Length).First().Length;
                    if (maxContinuousLineLengthInChunk > maxLineLength)
                    {
                        BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                        builder.AddItem($"Skipping chunk with continuous line length of {maxContinuousLineLengthInChunk}, too long to process.", "log_parse_error");
                        continue;
                    }
                }

                // look for following matches
                // at org.jenkinsci
                // at jenkins.
                // at hudson.
                // at java
                MatchCollection matches = new Regex(InternalErrorRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(chunk);
                if (matches.Any())
                {
                    BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                    foreach (Match match in matches)
                    {
                        builder.AddItem(match.Value, "internalError");
                        builder.NewLine();
                    }

                    result.Append(builder.GetText());
                }

                matches = new Regex(TimeoutRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(chunk);
                if (matches.Any())
                {
                    BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                    foreach (Match match in matches)
                    {
                        builder.AddItem(match.Value, "buildTimeout");
                        builder.NewLine();
                    }

                    result.Append(builder.GetText());
                }
            }

            string flattened = result.ToString();
            cache.Write(this, job, build, internalErrorHash, flattened);

            return flattened;
        }
    }
}
