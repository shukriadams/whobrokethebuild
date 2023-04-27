using System;

namespace Wbtb.Extensions.Data.Postgres
{
    public class PercentHelper
    {
        public static float ToPercent(int fraction, int total) 
        {
            if (total == 0)
                return 0;

            decimal percent = (decimal)fraction / (decimal)total;
            return (int)Math.Round((decimal)(percent * 100), 0);
        }
    }
}
