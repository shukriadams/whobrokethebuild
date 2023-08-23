using System;
using System.Linq;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.Cpp
{
    public class Cpp : Plugin, ILogParserPlugin
    {
        const string MSVCRegex = @"(.*\.h|.*\.cpp)(\(\d+,\d+\)): error\s?([A-Z]{1,2}[0-9]+):(.*)";

        const string ClangRegex = @"(.*\.h|.*\.cpp)(\(\d+,\d+\)): error\s?:(.*)([\s\S]*?generated\.)";

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
            string hash = Sha256.FromString(MSVCRegex + raw);
            Cache cache = di.Resolve<Cache>();
            string resultLookup = cache.Get(this, hash);
            if (resultLookup != null)
                return resultLookup;

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");
            string result = null;
            // Microsoft Visual C parsing
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
            MatchCollection matches = new Regex(MSVCRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Matches(fullErrorLog);
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

            /*
                clang parsing

                example :
                    In file included from ../../Cake/Intermediate/Build/Dev/Cake/Module.Cake.38_of_50.cpp:16:
                    C:\workspace\Cake\Source\Cake\Private\Jumper\FallingSate.cpp(1870,57): error: '&&' within '||' [-Werror,-Wlogical-op-parentheses]
                    if (fml == wtf)
                                                  ~~ ~~~~~~~~~~~~~~~~~~~~~~~~^~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                    C:\workspace\Cake\Source\Cake\Private\Jumper\FallingSate.cpp(1870,57): note: place parentheses around the '&&' expression to silence this warning
                        if (fml == wtf)
                          ^
                        (                                                                                          )
                    1 error generated.

            */
            matches = new Regex(ClangRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Matches(fullErrorLog);
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

            if (result == null)
                result = string.Empty;

            cache.Write(this, hash, result);
            return result;
        }
    }
}
