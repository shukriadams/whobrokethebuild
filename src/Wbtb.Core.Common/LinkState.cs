namespace Wbtb.Core.Common
{
    /// <summary>
    /// Used to mark status of complex link that is normally done in stages after a record is created, and which must be allowed to soft-fail repeatedly before eventually succeeding or failing.
    /// </summary>
    public enum LinkState
    {
        Queued = 0,
        Restarted = 1,
        Completed = 2,
        Failing = 3,
        Abandoned = 4
    }

}
