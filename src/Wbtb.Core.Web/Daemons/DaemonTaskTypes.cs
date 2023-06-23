namespace Wbtb.Core.Web.Daemons
{
    public enum DaemonTaskTypes
    {
        BuildEnd,
        AddBuildRevisionsFromBuildServer,
        RetrieveLog,
        AssignIncident,
        AddBuildRevisionsFromBuildLog,
        ResolveUser,
        ResolveRevision,
        ParseLog,
        AssignBlame,
        CalculateDelta,
        AlertDeltaChange
    }
}
