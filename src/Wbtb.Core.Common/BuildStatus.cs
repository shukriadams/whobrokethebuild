namespace Wbtb.Core.Common
{
    public enum BuildStatus: int
    {
        Unknown     = 0, // default
        Queued      = 1,
        InProgress  = 2,
        Failed      = 3,
        Passed      = 4,
        Aborted     = 5
    }
}
