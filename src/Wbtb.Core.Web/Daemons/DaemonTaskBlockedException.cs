using System;

namespace Wbtb.Core.Web
{
    public class DaemonTaskBlockedException : Exception
    {
        public DaemonTaskBlockedException(string message) : base(message) 
        {

        }
    }
}
