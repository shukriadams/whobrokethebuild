using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.Unreal
{
    /// <summary>
    /// Parses out errors in Unreal project content. These errors typically have a file path that starts in the UProject "game/content" directory.
    /// F.egs, a blueprint error will look like
    /// 
    /// Blueprint failed to compile: /Game/some/dir/BP_somefile.BP_somefile
    /// 
    /// In this case the file path is <workspace>/Game/content/some/dir/BP_somefile.uasset
    /// 
    /// Shader paths are also straight forward - the shader path in the error log corresponds to the actual file path. We identifiy shader extensions from the .usf extension.
    /// 
    /// 
    /// 
    /// </summary>
    public class Unreal4 : Plugin, ILogParserPlugin
    {
        private static string Find(string text, string regexPattern, RegexOptions options = RegexOptions.None, string defaultValue = "")
        {
            Match match = new Regex(regexPattern, options).Match(text);
            if (!match.Success || match.Groups.Count < 2)
                return defaultValue;

            return match.Groups[1].Value;
        }

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

            string bluePrintRegex4 = @"Blueprint failed to compile: (.*)";
            string bluePrintRegex5 = @"LogBlueprint: Error: \[AssetLog\] .*?: \[Compiler]\ (.*)? from Source: (.*)?";
            string shaderRegex = @"LogShaderCompilers: Warning:\n*(.*?.usf)\(\): Shader (.*?), .*";

            // force Unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            IEnumerable<string> chunks = null;
            if (string.IsNullOrEmpty(chunkDelimiter))
                chunks = new List<string> { fullErrorLog };
            else
                chunks = fullErrorLog.Split(chunkDelimiter);

            // try for cache
            string shaderRegexHash = Sha256.FromString(bluePrintRegex4 + bluePrintRegex5 + shaderRegex + raw);
            Cache cache = di.Resolve<Cache>();
            CachePayload shaderMatchLookup = cache.Get(this, job, build, shaderRegexHash);
            if (shaderMatchLookup != null)
                return shaderMatchLookup.Payload;

            StringBuilder allMatches = new StringBuilder();

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

                // try for cache
                string blueprint4RegexHash = Sha256.FromString(bluePrintRegex4 + raw);
                string blueprint5RegexHash = Sha256.FromString(bluePrintRegex5 + raw);

                MatchCollection matches = new Regex(bluePrintRegex4, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Matches(chunk);
                if (matches.Any())
                {
                    BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig.Manifest.Key);

                    // always add flag at start of log data
                    builder.AddItem("blueprint", "flag");

                    foreach (Match match in matches)
                    {
                        builder.AddItem(match.Groups[1].Value, "path");
                        builder.AddItem(string.Empty, "description"); //write empty description to match v5 error format below
                        builder.NewLine();
                    }

                    allMatches.Append(builder.GetText());
                }

                matches = new Regex(bluePrintRegex5, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Matches(chunk);
                if (matches.Any())
                {
                    BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig.Manifest.Key);

                    // always add flag at start of log data
                    builder.AddItem("blueprint", "flag");

                    foreach (Match match in matches)
                    {
                        builder.AddItem(match.Groups[2].Value, "path");
                        builder.AddItem(match.Groups[1].Value, "description");
                        builder.NewLine();
                    }

                    allMatches.Append(builder.GetText());
                }

                // shaders
                matches = new Regex(shaderRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Matches(chunk);
                if (matches.Any())
                {
                    BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig.Manifest.Key);

                    // always add flag at start of log data
                    builder.AddItem("shader", "flag");

                    foreach (Match match in matches)
                    {
                        builder.AddItem(match.Groups[1].Value, "path");
                        builder.AddItem(match.Groups[1].Value, "shader");
                        builder.NewLine();
                    }

                    allMatches.Append(builder.GetText());
                }
            }

            string flattened = allMatches.ToString();
            cache.Write(this, job, build, shaderRegexHash, flattened);
            return flattened;
        }
    }
}
