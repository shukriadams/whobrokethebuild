using System;
using System.Collections.Generic;
using System.Text;

namespace Wbtb.Core.Common
{
    public enum BuildProcessorStatus: int
    {
        Pending = 0,
        Passed = 1,
        Failed = 3,
        Cancelled = 4
    }
}
