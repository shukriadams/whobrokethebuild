using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Extensions.Auth.ActiveDirectory;
using Wbtb.Extensions.Auth.ActiveDirectorySandbox;
using Wbtb.Extensions.BuildServer.Jenkins;
using Wbtb.Extensions.Data.Postgres;
using Wbtb.Extensions.LogParsing.Cpp;
using Wbtb.Extensions.LogParsing.JenkinsSelfFailing;
using Wbtb.Extensions.LogParsing.Unreal;
using Wbtb.Extensions.Messaging.Slack;
using Wbtb.Extensions.SourceServer.Perforce;
using Wbtb.Extensions.SourceServer.PerforceSandbox;

namespace Wbtb.Core.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            try 
            {
                // bind types - dev only! These are needed by all general plugin activity
                SimpleDI di = new SimpleDI();

                di.Register<IDataLayerPlugin, Postgres>();
                di.Register<IAuthenticationPlugin, ActiveDirectory>();
                di.Register<IAuthenticationPlugin, ActiveDirectorySandbox>();
                di.Register<IBuildServerPlugin, Jenkins>();
                di.Register<IBuildServerPlugin, Extensions.BuildServer.JenkinsSandbox.JenkinsSandbox>();
                di.Register<IPostProcessor, Extensions.PostProcessing.Test.Test>();
                di.Register<IPostProcessor, Extensions.PostProcessing.Test2.Test2>();
                di.Register<ISourceServerPlugin, Perforce>();
                di.Register<ISourceServerPlugin, PerforceSandbox>();
                di.Register<ILogParser, Unreal4>();
                di.Register<ILogParser, JenkinsSelfFailing>();
                di.Register<ILogParser, Cpp>();
                di.Register<IMessaging, Slack>();
                di.Register<ConfigurationBuilder, ConfigurationBuilder>();
                di.Register<OrphanRecordHelper, OrphanRecordHelper>();
                di.Register<PluginManager, PluginManager>();
                di.Register<PluginProvider, PluginProvider>();
                di.Register<OrphanRecordHelper, OrphanRecordHelper>();
                di.Register<ConfigBootstrapper, ConfigBootstrapper>();

                ConfigBootstrapper configBootstrapper = di.Resolve<ConfigBootstrapper>();
                CustomEnvironmentArgs customEnvironmentArgs = di.Resolve<CustomEnvironmentArgs>();
                customEnvironmentArgs.Apply();
                if (!configBootstrapper.EnsureLatest())
                    Environment.Exit(0);

                Config config = di.Resolve<Config>();

                ConfigurationBuilder configurationBuilder = di.Resolve<ConfigurationBuilder>();
                OrphanRecordHelper orphanRecordHelper = di.Resolve<OrphanRecordHelper>();
                PluginProvider pluginProvider = di.Resolve<PluginProvider>();
                //Core.LoadConfig(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "config.yml"));
                //Core.LoadPlugins();

                CommandLineSwitches switches = new CommandLineSwitches(args);
                
                // list jobs on buildserver
                if (switches.Contains("IBuildServerPlugin.ListRemoteJobsCanonical"))
                {
                    if (!switches.Contains("Key"))
                    {
                        Console.WriteLine($"ERROR : \"Key\" arg required, for buildserver Key to list jobs for");
                        Environment.Exit(1);
                    }
                        
                    string buildServerKey = switches.Get("Key");
                    BuildServer buildServer = config.BuildServers.FirstOrDefault(r => r.Key == buildServerKey);
                    if (buildServer == null)
                    {
                        Console.WriteLine($"ERROR : Buildserver with key \"{buildServerKey}\" not found");
                        Environment.Exit(1);
                    }

                    IBuildServerPlugin buildServerPlugin = pluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
                    IEnumerable<string> jobs = buildServerPlugin.ListRemoteJobsCanonical(buildServer);
                    Console.WriteLine($"Found {jobs.Count()} on buildserver \"{buildServer.Key}\".");

                    foreach (string job in jobs)
                    {
                        Console.WriteLine($"{job}");
                    }
                }

                
                // list orphan records
                if (switches.Contains("IDataLayerPlugin.ListOrphanedRecords"))
                { 
                    Console.WriteLine("Executing function IDataLayerPlugin.ListOrphanedRecords");

                    IEnumerable<string> orphans = configurationBuilder.FindOrphans();
                    foreach(string orphan in orphans)
                        Console.WriteLine(orphan);
                }

                // merge orphan source server records
                if (switches.Contains("IDataLayerPlugin.MergeSourceServers"))
                {
                    Console.WriteLine("Executing function IDataLayerPlugin.MergeSourceServers");
                    if (!switches.Contains("from"))
                    {
                        Console.WriteLine($"ERROR : key \"from\" required");
                        Environment.Exit(1);
                    }

                    if (!switches.Contains("to"))
                    {
                        Console.WriteLine($"ERROR : key \"to\" required");
                        Environment.Exit(1);
                    }

                    string fromSourceServerKey = switches.Get("from");
                    string toSourceServerKey = switches.Get("to");

                    try 
                    {
                        orphanRecordHelper.MergeSourceServers(fromSourceServerKey, toSourceServerKey);
                    }
                    catch(RecordNotFoundException ex)
                    {
                        Console.WriteLine(ex.Message);
                        Environment.Exit(1);
                    }
                }

                // merge orphan build server records
                if (switches.Contains("IDataLayerPlugin.MergeBuildServers"))
                {
                    Console.WriteLine("Executing function IDataLayerPlugin.MergeBuildServers");
                    if (!switches.Contains("from"))
                    {
                        Console.WriteLine($"ERROR : key \"from\" required");
                        Environment.Exit(1);
                    }

                    if (!switches.Contains("to"))
                    {
                        Console.WriteLine($"ERROR : key \"to\" required");
                        Environment.Exit(1);
                    }


                    string fromBuildServerKey = switches.Get("from");
                    string toBuildServerKey = switches.Get("to");

                    try
                    {
                        orphanRecordHelper.MergeBuildServers(fromBuildServerKey, toBuildServerKey);
                    }
                    catch (RecordNotFoundException ex)
                    {
                        Console.WriteLine(ex.Message);
                        Environment.Exit(1);
                    }
                }

                // merge orphan user records
                if (switches.Contains("IDataLayerPlugin.MergeUsers"))
                {
                    Console.WriteLine("Executing function IDataLayerPlugin.MergeUsers");
                    if (!switches.Contains("from"))
                    {
                        Console.WriteLine($"ERROR : key \"from\" required");
                        Environment.Exit(1);
                    }

                    if (!switches.Contains("to"))
                    {
                        Console.WriteLine($"ERROR : key \"to\" required");
                        Environment.Exit(1);
                    }

                    string fromUserKey = switches.Get("from");
                    string toUserKey = switches.Get("to");

                    try
                    {
                        orphanRecordHelper.MergeUsers(fromUserKey, toUserKey);
                    }
                    catch (RecordNotFoundException ex)
                    {
                        Console.WriteLine(ex.Message);
                        Environment.Exit(1);
                    }
                }

                // delete orphan user records
                if (switches.Contains("IDataLayerPlugin.DeleteUser"))
                {
                    Console.WriteLine("Executing function IDataLayerPlugin.DeleteUser");
                    if (!switches.Contains("key"))
                    {
                        Console.WriteLine($"ERROR : key required");
                        Environment.Exit(1);
                    }


                    string key = switches.Get("key");

                    try
                    {
                        orphanRecordHelper.DeleteUser(key);
                    }
                    catch (RecordNotFoundException ex)
                    {
                        Console.WriteLine(ex.Message);
                        Environment.Exit(1);
                    }
                }

                // delete orphan source serverrecords
                if (switches.Contains("IDataLayerPlugin.DeleteSourceServer"))
                {
                    Console.WriteLine("Executing function IDataLayerPlugin.DeleteSourceServer");
                    if (!switches.Contains("key"))
                    {
                        Console.WriteLine($"ERROR : key required");
                        Environment.Exit(1);
                    }


                    string key = switches.Get("key");

                    try
                    {
                        orphanRecordHelper.DeleteSourceServer(key);
                    }
                    catch (RecordNotFoundException ex)
                    {
                        Console.WriteLine(ex.Message);
                        Environment.Exit(1);
                    }
                }

                // delete orphan source serverrecords
                if (switches.Contains("IDataLayerPlugin.DeleteBuildServer"))
                {
                    Console.WriteLine("Executing function IDataLayerPlugin.DeleteBuildServer");
                    if (!switches.Contains("key"))
                    {
                        Console.WriteLine($"ERROR : key required");
                        Environment.Exit(1);
                    }

                    string key = switches.Get("key");

                    try
                    {
                        orphanRecordHelper.DeleteBuildServer(key);
                    }
                    catch (RecordNotFoundException ex)
                    {
                        Console.WriteLine(ex.Message);
                        Environment.Exit(1);
                    }
                }

                Console.WriteLine("WBTB CLI exited normally.");
            }
            catch (ConfigurationException ex)
            {
                Console.WriteLine($"CONFIG ERROR : {ex.Message}");
            }
        }
    }
}
