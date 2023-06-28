using System;
using System.Globalization;

namespace Wbtb.Core.Web
{
    public static class TimeExtensions
    {

        public static string Ago(this DateTime? beforeUtc)
        {
            if (beforeUtc == null)
                return string.Empty;

            return _ago(beforeUtc.Value);
        }

        public static string Ago(this DateTime beforeUtc)
        {
            return _ago(beforeUtc);
        }

        private static string _ago(DateTime beforeUtc) 
        {
            TimeSpan ts = DateTime.Now - beforeUtc.ToLocalTime();
            int count = 0;

            if (ts.TotalDays > 364)
            {
                count = (int)Math.Round(ts.TotalDays / 364, 0);
                return $"{count} year" + (count == 1 ? "" : $"s");
            }

            if (ts.TotalHours > 24)
            {
                count = (int)Math.Round(ts.TotalDays, 0);
                return $"{count} day" + (count == 1 ? "" : $"s");
            }

            if (ts.TotalMinutes > 24)
            {
                count = (int)Math.Round(ts.TotalHours, 0);
                return $"{count} hour" + (count == 1 ? "" : $"s");
            }

            count = (int)Math.Round(ts.TotalSeconds, 0);
            return $"{count} second" + (count == 1 ? "" : $"s");
        }

        public static string ToShort(this DateTime dateUtc)
        {

            DateTime date = dateUtc.ToLocalTime();
            
            string shortened = date.ToString("d", CultureInfo.CurrentCulture);
            return shortened;

        }

        public static string ToShort(this DateTime? dateUtc)
        {
            if (!dateUtc.HasValue)
                return string.Empty;

            DateTime date = dateUtc.Value.ToLocalTime();

            string shortened = date.ToString("d", CultureInfo.CurrentCulture);
            return shortened;
        }

    }
}
