namespace Wbtb.Core.Web.Daemons
{
    public enum DaemonTaskTypes
    {
        BuildEnd,
        AddBuildRevisionsFromBuildServer,
        RetrieveLog,
        AssignIncident,
        AddBuildRevisionsFromBuildLog,
        ResolveUsers,
        ParseLog,
        AssignBlame,
        CalculateDelta,
        AlertDeltaChange
    }
}
