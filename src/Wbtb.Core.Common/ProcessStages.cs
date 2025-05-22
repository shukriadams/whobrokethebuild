namespace Wbtb.Core.Common
{
    public enum ProcessStages : int
    {
        BuildEnd = 0,
        RevisionFromBuildServer = 1,
        IncidentAssign = 2,
        LogImport = 3,
        RevisionFromLog = 4,
        RevisionLink = 5,
        UserLink = 6,
        LogParse = 7,
        PostProcess = 8,
        Alert = 9
    }
}
