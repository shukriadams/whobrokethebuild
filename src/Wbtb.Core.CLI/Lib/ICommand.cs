using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal interface ICommand
    {
        string Describe();
        void Process(CommandLineSwitches switches);
    }
}
