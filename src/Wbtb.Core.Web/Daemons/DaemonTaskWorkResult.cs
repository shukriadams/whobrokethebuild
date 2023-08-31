namespace Wbtb.Core.Web
{
    public enum DaemonTaskWorkResultType
    {
        Failed,
        WriteCollision,
        Passed, 
        Blocked
    }

    public class DaemonTaskWorkResult
    {
        public DaemonTaskWorkResultType ResultType { get; set; }

        public string Description { get; set; }

        public DaemonTaskWorkResult() 
        {
            this.ResultType = DaemonTaskWorkResultType.Passed;
        }
    }
}
