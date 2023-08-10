using System;
using System.Linq;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.Cpp
{
    public class Cpp : Plugin, ILogParserPlugin
    {
        const string Regex = @"(.*?)(\(\d+,\d+\)): error\s?([A-Z]{1,2}[0-9]+):(.*)";

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

            // try for cache
            string hash = Sha256.FromString(Regex + raw);
            Cache cache = di.Resolve<Cache>();
            string resultLookup = cache.Get(this, hash);
            if (resultLookup != null)
                return resultLookup;

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            // try to parse out C++ compile errors from log, these errors have the form like
            // D:\some\path\file.h(41,12): error C2143: syntax error: missing ';' before '*'
            // file path(line number): error code: description
            //
            // Groups
            // 0 : all
            // 1 : file path
            // 2 : line nr in file
            // 3 : error code
            // 4 : description
            MatchCollection matches = new Regex(Regex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Matches(fullErrorLog);
            string result = string.Empty;
            if (matches.Any())
            {
                BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig.Manifest.Key);

                foreach (Match match in matches)
                {
                    builder.AddItem(match.Groups[1].Value, "path");
                    builder.AddItem(match.Groups[2].Value, "line_number");
                    builder.AddItem(match.Groups[3].Value, "error_code");
                    builder.AddItem(match.Groups[4].Value, "description");
                    builder.NewLine();
                }

                result = builder.GetText();
            }

            cache.Write(this, hash, result);
            return result;
        }
    }
}
