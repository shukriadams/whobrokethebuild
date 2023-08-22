using System;
using System.Linq;
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

        string ILogParserPlugin.Parse(string raw)
        {
            SimpleDI di = new SimpleDI();

            string bluePrintRegex4 = @"Blueprint failed to compile: (.*)";
            string bluePrintRegex5 = @"LogBlueprint: Error: \[AssetLog\] .*?: \[Compiler]\ (.*)? from Source: (.*)?";

            // try for cache
            string blueprint4RegexHash = Sha256.FromString(bluePrintRegex4 + raw);
            string blueprint5RegexHash = Sha256.FromString(bluePrintRegex5 + raw);
            Cache cache = di.Resolve<Cache>();
            string bluePrintMatch = cache.Get(this, blueprint4RegexHash);

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            // try blueprint 4x format
            if (bluePrintMatch == null)
            {
                MatchCollection matches = new Regex(bluePrintRegex4, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Matches(fullErrorLog);
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

                    bluePrintMatch = builder.GetText();
                    cache.Write(this, blueprint4RegexHash, bluePrintMatch);
                }
            }

            // try blueprint 5x format
            if (bluePrintMatch == null)
            {
                MatchCollection matches = new Regex(bluePrintRegex5, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Matches(fullErrorLog);
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

                    bluePrintMatch = builder.GetText();
                    cache.Write(this, blueprint5RegexHash, bluePrintMatch);
                }
            }

            // no blueprint matches found, write both to cache so we don't reprocess
            if (bluePrintMatch == null) 
            {
                bluePrintMatch = string.Empty;
                cache.Write(this, blueprint4RegexHash, bluePrintMatch);
                cache.Write(this, blueprint5RegexHash, bluePrintMatch);
            }

            string shaderRegex = @"LogShaderCompilers: Warning:\n*(.*?.usf)\(\): Shader (.*?), .*";

            // try for cache
            string shaderRegexHash = Sha256.FromString(shaderRegex + raw);
            string shaderMatch = cache.Get(this, shaderRegexHash);
            if (shaderMatch == null)
            {
                MatchCollection matches = new Regex(shaderRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Matches(fullErrorLog);
                shaderMatch = string.Empty;
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
                    
                    shaderMatch = builder.GetText();
                }

                cache.Write(this, shaderRegexHash, shaderMatch);
            }

            return bluePrintMatch + shaderMatch;
        }
    }
}
