using Wbtb.Core.Common;

namespace Wbtb.Extensions.Develop.DataOverTime
{
    public class DataOverTime : Plugin, IDataOverTime
    {
        ReachAttemptResult IReachable.AttemptReach()
        {
            return new ReachAttemptResult { Reachable = true };
        }

        void IPlugin.Diagnose()
        {
            // not used
        }

        string IDataOverTime.GetTime()
        {
            return "current time is X";
        }

        PluginInitResult IPlugin.InitializePlugin()
        {
            return new PluginInitResult { Success = true };
        }

        void IDataOverTime.Reset()
        {
            Console.WriteLine("Not implemented");
        }

        void IDataOverTime.SetTime(string time)
        {
            Console.WriteLine();
        }
    }
}
