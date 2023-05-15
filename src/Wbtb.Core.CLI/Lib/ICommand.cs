using Wbtb.Core.Common;

namespace Wbtb.Core.CLI
{
    internal interface ICommand
    {
        void Process(CommandLineSwitches switches);
    }
}
