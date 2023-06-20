namespace Wbtb.Core.Web.Daemons
{
    public enum DaemonTaskTypes
    {
        BuildEnd,
        AddBuildRevisions,
        RetrieveLog,
        AssignIncident,
        ResolveRevisions,
        ResolveUsers,
        ParseLog,
        AssignBlame,
        CalculateDelta,
        AlertDeltaChange
    }
}
