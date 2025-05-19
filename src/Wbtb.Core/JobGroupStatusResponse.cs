using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public class JobGroupStatusResponse
    {
        public bool Success { get; set; }
        
        public string Message { get; set; }

        public JobGroupStatus? Status { get; set; }
    }
}
