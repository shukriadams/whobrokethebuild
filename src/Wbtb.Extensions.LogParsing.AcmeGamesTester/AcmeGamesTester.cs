using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.AcmeGamesTester
{
    public class AcmeGamesTester : Plugin, ILogParserPlugin
    {
        #region PROPERTIES

        private readonly ILogger _log;

        #endregion

        #region CTORS

        public AcmeGamesTester(ILogger log)
        {
            _log = log;
        }

        #endregion

        #region METHODS

        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult
            {
                SessionId = Guid.NewGuid().ToString(),
                Success = true
            };
        }

        string ILogParserPlugin.Parse(Build build, string raw)
        {
            int maxLogSize = 0;
            if (ContextPluginConfig.Config.Any(r => r.Key == "MaxLogSize"))
                int.TryParse(ContextPluginConfig.Config.First(r => r.Key == "MaxLogSize").Value.ToString(), out maxLogSize);

            if (maxLogSize > 0 && raw.Length > maxLogSize)
                return $"Log length ({raw.Length}) exceeds max allowed parse length ({maxLogSize}).";

            // example of an error :
            // LogLinker: Error: [AssetLog] D:\workdir\IronBird\Content\Acme\Tests\ThingTester.umap: Failed import: class 'AngelGrinder' name '_Sprucker' outer 'BP_Luncher'. There is another object (of 'BP_Luncher' class) at the path."
            // group 1 : filepath
            // group 2 : error explanation
            string errorRegex = @"LogLinker: Error: \[.*?\] (.*)?: (.*?: .*)";

            SimpleDI di = new SimpleDI();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);
            Cache cache = di.Resolve<Cache>();

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            // internal error - try for cache
            string errorHash = Sha256.FromString(errorRegex + fullErrorLog);
            CachePayload internaleErrorCacheLookup = cache.Get(this, job, build, errorHash);
            if (internaleErrorCacheLookup.Payload != null)
                return internaleErrorCacheLookup.Payload;

            MatchCollection matches = new Regex(errorRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(fullErrorLog);
            string result = string.Empty;
            if (matches.Any())
            {
                BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig);
                foreach (Match match in matches)
                {
                    builder.AddItem(match.Groups[1].Value, "path");
                    builder.AddItem(match.Groups[2].Value, "description");
                    builder.NewLine();
                }

                result = builder.GetText();
                cache.Write(this, job, build, errorHash, result);
            }

            return result;
        }

        #endregion
    }
}
