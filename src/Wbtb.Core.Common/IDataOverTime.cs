using System;

namespace Wbtb.Core.Common
{
    public interface IDataOverTime: IReachable, IPlugin 
    {
        void SetTime(string time);
     
        string GetTime();
        
        void Reset();
    }
}
