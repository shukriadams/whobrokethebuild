using System;

namespace Wbtb.Core.Web
{
    public class DaemonTaskFailedException : Exception
    {
        public DaemonTaskFailedException(string message) : base(message)
        {

        }
    }
}
