using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal class IDataLayer_TaskQueueBuild
    {
        public string Describe()
        {
            return @"Deletes a job from database. This is meant for orphan cleanup";
        }

        public void Process(CommandLineSwitches switches)
        {
            SimpleDI di = new SimpleDI();
            PluginProvider _pluginProvider = di.Resolve<PluginProvider>();
            IDataPlugin datalayer = _pluginProvider.GetFirstForInterface<IDataPlugin>();

            if (!switches.Contains("build"))
            {
                Console.WriteLine($"ERROR : --build <buildid> required");
                Environment.Exit(1);
                return;
            }

            string buildid = switches.Get("build");
            Build build = datalayer.GetBuildById(buildid);
            if (build == null) 
            {
                Console.WriteLine($"ERROR : build with id \"{buildid}\" not found.");
                Environment.Exit(1);
                return;
            }

            string taskName = "BuildEnd";

            if (datalayer.GetDaemonsTaskByBuild(buildid).Any(t => t.TaskKey == taskName)) 
            {
                Console.WriteLine($"Build {buildid} already queued.");
                Environment.Exit(1);
                return;
            }

            datalayer.SaveDaemonTask(new DaemonTask
            {
                BuildId = buildid,
                Src = this.GetType().Name,
                TaskKey = taskName // TODO : hardcoded key here, all daemons need to be moved to core for proper fix
            });

            Console.WriteLine("Build queued to process");
        }
    }
}
