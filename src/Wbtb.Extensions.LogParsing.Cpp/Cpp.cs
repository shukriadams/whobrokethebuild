using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Extensions.LogParsing.Cpp
{
    public class Cpp : Plugin, ILogParserPlugin
    {
        const string MSVCRegex = @"(.*\.h|.*\.cpp)\((.*)\):.*?error\s?([A-Z]{1,2}[0-9]+)?:(.*)";
        // (.*\.h|.*\.cpp)(\(.*?\)):(.*?error.*)
        const string ClangRegex = @"(.*\.h|.*\.cpp)(\(\d+,\d+\)): error\s?:(.*)([\s\S]*?generated\.)";

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
            ILogger logger = di.Resolve<ILogger>();
            PluginProvider pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();
            Job job = dataLayer.GetJobById(build.JobId);

            // try for cache
            string hash = Sha256.FromString(MSVCRegex + ClangRegex + raw);
            Cache cache = di.Resolve<Cache>();
            CachePayload resultLookup = cache.Get(this, job, build, hash);
            if (resultLookup.Payload != null)
            {
                logger.LogInformation($"{this.GetType().Name} found cache hit {resultLookup.Key}.");
                return resultLookup.Payload;
            }

            // force unix paths on log, this helps reduce noise when getting distinct lines
            string fullErrorLog = raw.Replace("\\", "/");

            IEnumerable<string> chunks = null;
            if (string.IsNullOrEmpty(chunkDelimiter))
                chunks = new List<string> { fullErrorLog };
            else
                chunks = fullErrorLog.Split(chunkDelimiter);

            StringBuilder result = new StringBuilder();

            foreach (string chunk in chunks)
            {
                // filter out chunks that can poison regex with overly-long sequences
                if (maxLineLength > 0)
                {
                    int maxContinuousLineLengthInChunk = chunk.Split(" ").OrderByDescending(r => r.Length).First().Length;
                    if (maxContinuousLineLengthInChunk > maxLineLength)
                    {
                        BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig.Manifest.Key);
                        builder.AddItem($"Skipping chunk with continuous line length of {maxContinuousLineLengthInChunk}, too long to process.", "log_parse_error");
                        continue;
                    }
                }


                /*
                    Microsoft Visual C parsing
                    try to parse out C++ compile errors from log, these errors have the form like
                    D:\some\path\file.h(41,12): error C2143: syntax error: missing ';' before '*'
                    file path(line number): error code: description
                
                    Groups
                    0 : all
                    1 : file path
                    2 : line nr in file
                    3 : error code (optional)
                    4 : description
                */
                MatchCollection matches = new Regex(MSVCRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Matches(chunk);
                if (matches.Any())
                {
                    BuildLogTextBuilder builder = new BuildLogTextBuilder(this.ContextPluginConfig.Manifest.Key);

                    foreach (Match match in matches)
                    {
                        builder.AddItem(match.Groups[1].Value, "path");
                        builder.AddItem(match.Groups[2].Value, "line_number");

                        if (match.Groups.Count == 4)
                        {
                            builder.AddItem(match.Groups[3].Value, "description");
                        }
                        else
                        {
                            builder.AddItem(match.Groups[3].Value, "error_code");
                            builder.AddItem(match.Groups[4].Value, "description");
                        }

                        builder.NewLine();
                    }

                    result.Append(builder.GetText());
                }

                /*
                    Clang parsing
                    Example :
                        In file included from ../../Cake/Intermediate/Build/Dev/Cake/Module.Cake.38_of_50.cpp:16:
                        C:\workspace\Cake\Source\Cake\Private\Jumper\FallingSlate.cpp(1870,57): error: '&&' within '||' [-Werror,-Wlogical-op-parentheses]
                        if (fml == wtf)
                                                      ~~ ~~~~~~~~~~~~~~~~~~~~~~~~^~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                        C:\workspace\Cake\Source\Cake\Private\Jumper\FallingSlate.cpp(1870,57): note: place parentheses around the '&&' expression to silence this warning
                            if (fml == wtf)
                              ^
                            (                                                                                          )
                        1 error generated.

                */
                matches = new Regex(ClangRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled).Matches(chunk);
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

                    result.Append(builder.GetText());
                }
            }

            string resultFlattened = result.ToString();
            cache.Write(this, job, build, hash, resultFlattened);

            return resultFlattened;
        }
    }
}
