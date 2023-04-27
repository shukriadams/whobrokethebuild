using System;

namespace Wbtb.Core.Common
{
    public class ReachAttemptResult
    {
        public bool Reachable { get;set;}

        /// <summary>
        /// If an exception was thrown, this will be set. Else, check error string
        /// </summary>
        public Exception Exception { get; set; }

        public string Error { get; set; }

        public ReachAttemptResult()
        { 
            Error = string.Empty;
        }
    }
}
