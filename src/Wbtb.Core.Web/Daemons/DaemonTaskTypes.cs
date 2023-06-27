﻿namespace Wbtb.Core.Web.Daemons
{
    public enum DaemonTaskTypes
    {
        BuildEnd,                           // 0
        AddBuildRevisionsFromBuildServer,   // 0
        LogImport,                          // 1
        IncidentAssign,                     // 1
        AddBuildRevisionsFromBuildLog,      // 2
        UserResolve,                        // 3
        RevisionResolve,                    // 3
        LogParse,                           // 3
        AssignBlame,                        // 4
        DeltaCalculate,                     // 5
        DeltaChangeAlert                    // 6
    }
}
