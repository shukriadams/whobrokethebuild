using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;
using Wbtb.Core;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.JustRegexPlease
{
    public class Describe 
    {
        public string Name { get; set; }
        public string Regex { get; set; }
    }

    /// <summary>
    /// Locally defined wrapper for expected plugin config
    /// </summary>
    public class ThisPluginConfig
    {
        public IList<Describe> Describes { get; set; } = new List<Describe>();
        public string Name { get; set; }
        public string SectionDelimiter { get; set; }
        public string Regex { get; set; }
    }

    public class JustRegexPlease : Plugin, ILogParserPlugin
    {
        #region PROPERTIES

        private readonly ILogger _log;

        #endregion

        #region CTORS

        public JustRegexPlease(ILogger log)
        {
            _log = log;
        }

        #endregion

        #region METHODS

        PluginInitResult IPlugin.InitializePlugin()
        {
            Response<ThisPluginConfig> response = this.ContextPluginConfig.Deserialize<ThisPluginConfig>("Custom");
            if (response.Error != null)
                throw new ConfigurationException($"Could not deserialize plugin config {this.ContextPluginConfig.Key} to type {TypeHelper.Name<ThisPluginConfig>()} : {response.Error}");

            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }


        string ILogParserPlugin.Parse(Build build, string raw)
        {
            Response<ThisPluginConfig> response = this.ContextPluginConfig.Deserialize<ThisPluginConfig>("Custom");

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            Cache cache = di.Resolve<Cache>();

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            // internal error - try for cache
            string errorHash = Sha256.FromString(response.Value.Regex + fullErrorLog);
            CachePayload internaleErrorCacheLookup = cache.Get(this, job, build, errorHash);
            if (internaleErrorCacheLookup.Payload != null)
                return internaleErrorCacheLookup.Payload;

            // try to break log up into chunks for better performance
            IEnumerable<string> chunks;
            if (string.IsNullOrEmpty(response.Value.SectionDelimiter))
                chunks = new List<string> { fullErrorLog };
            else
                chunks = fullErrorLog.Split(response.Value.SectionDelimiter);

            StringBuilder result = new StringBuilder();
            foreach (string chunk in chunks)
            {
                MatchCollection matches = new Regex(response.Value.Regex, RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(chunk);
                if (matches.Any())
                {
                    // main regex tripped, try to match descibers now
                    foreach (Describe describe in response.Value.Describes) 
                    {
                        MatchCollection describeMatches = new Regex(describe.Regex, RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(chunk);
                        if (describeMatches.Any()) 
                        {
                            BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                            foreach (Match match in describeMatches)
                            {
                                builder.AddItem(describe.Name, "name");
                                builder.AddItem(match.Groups[1].Value, "value");
                                builder.NewLine();
                            }

                            result.Append(builder.GetText());
                        }
                    }
                }
            }

            string flattened = result.ToString();
            cache.Write(this, job, build, errorHash, flattened);
            return flattened;
        }

        #endregion
    }
}
