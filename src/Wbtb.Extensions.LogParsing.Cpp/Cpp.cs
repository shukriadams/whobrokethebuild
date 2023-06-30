using System;
using System.Linq;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.Cpp
{
    public class Cpp : Plugin, ILogParserPlugin
    {
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
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            SimpleDI di = new SimpleDI();

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            string regex = @"(.*?)(\(\d+,\d+\)): error\s?([A-Z]{1,2}[0-9]+):(.*)";
            string hash = Sha256.FromString(regex + fullErrorLog);
            Cache cache = di.Resolve<Cache>();
            string resuleLookup = cache.Get(hash);
            if (resuleLookup != null)
                return resuleLookup;

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
            MatchCollection matches = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Multiline).Matches(fullErrorLog);

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

                resuleLookup = builder.GetText();
            }
            else 
            {
                resuleLookup = string.Empty;
            }

            cache.Write(hash, resuleLookup);
            return resuleLookup;
        }
    }
}
