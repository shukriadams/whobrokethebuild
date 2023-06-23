namespace Wbtb.Core.Web.Daemons
{
    public enum DaemonTaskTypes
    {
        BuildEnd,
        AddBuildRevisionsFromBuildServer,
        LogImport,
        IncidentAssign,
        AddBuildRevisionsFromBuildLog,
        UserResolve,
        RevisionResolve,
        LogParse,
        AssignBlame,
        DeltaCalculate,
        DeltaChangeAlert
    }
}
