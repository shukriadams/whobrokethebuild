using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using Wbtb.Core.Common;
using Wbtb.Core.Common.Plugins;
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
                StandardKernel kernel = new StandardKernel();
                kernel.Bind<IDataLayerPlugin>().To<Postgres>();
                kernel.Bind<IAuthenticationPlugin>().To<ActiveDirectory>();
                kernel.Bind<IAuthenticationPlugin>().To<ActiveDirectorySandbox>();
                kernel.Bind<IBuildServerPlugin>().To<Jenkins>();
                kernel.Bind<IBuildServerPlugin>().To<Extensions.BuildServer.JenkinsSandbox.JenkinsSandbox>();
                kernel.Bind<IPostProcessor>().To<Extensions.PostProcessing.Test.Test>();
                kernel.Bind<IPostProcessor>().To<Extensions.PostProcessing.Test2.Test2>();
                kernel.Bind<ISourceServerPlugin>().To<Perforce>();
                kernel.Bind<ISourceServerPlugin>().To<PerforceSandbox>();
                kernel.Bind<ILogParser>().To<Unreal4>();
                kernel.Bind<ILogParser>().To<JenkinsSelfFailing>();
                kernel.Bind<ILogParser>().To<Cpp>();
                kernel.Bind<IMessaging>().To<Slack>();
                PluginProvider.Factory = new NinjectWrapper(kernel);

                CustomEnvironmentArgs.Apply();
                if (!ConfigBootstrapper.EnsureLatest())
                    Environment.Exit(0);

                throw new NotImplementedException("fix this");
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
                    BuildServer buildServer = ConfigKeeper.Instance.BuildServers.FirstOrDefault(r => r.Key == buildServerKey);
                    if (buildServer == null)
                    {
                        Console.WriteLine($"ERROR : Buildserver with key \"{buildServerKey}\" not found");
                        Environment.Exit(1);
                    }

                    IBuildServerPlugin buildServerPlugin = PluginProvider.GetByKey(buildServer.Plugin) as IBuildServerPlugin;
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

                    IEnumerable<string> orphans = Configuration.ConfigurationBuilder.FindOrphans();
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
                        OrphanRecordHelper.MergeSourceServers(fromSourceServerKey, toSourceServerKey);
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
                        OrphanRecordHelper.MergeBuildServers(fromBuildServerKey, toBuildServerKey);
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
                        OrphanRecordHelper.MergeUsers(fromUserKey, toUserKey);
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
                        OrphanRecordHelper.DeleteUser(key);
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
                        OrphanRecordHelper.DeleteSourceServer(key);
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
                        OrphanRecordHelper.DeleteBuildServer(key);
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
