using System;

namespace Wbtb.Core.Common
{
    public static class DateTimeExtensions
    {
        public static string Ago(this DateTime? beforeUtc)
        {
            if (beforeUtc == null)
                return string.Empty;

            return _ago(beforeUtc.Value);
        }

        public static string ToHumanString(this TimeSpan ts) 
        {
            return _ago(ts);
        }
        
        public static string ToHumanString(this TimeSpan? ts)
        {
            if (ts == null)
                return string.Empty;
            return _ago(ts.Value);
        }

        public static string Ago(this DateTime beforeUtc)
        {
            return _ago(beforeUtc);
        }

        private static string _ago(DateTime beforeUtc)
        {
            return _ago(DateTime.Now - beforeUtc.ToLocalTime());
        }

        private static string _ago(TimeSpan ts)
        {
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

            if (ts.TotalMinutes > 60)
            {
                count = (int)Math.Round(ts.TotalHours, 0);
                return $"{count} hour" + (count == 1 ? "" : $"s");
            }

            if (ts.TotalSeconds > 60)
            {
                count = (int)Math.Round(ts.TotalMinutes, 0);
                return $"{count} minute" + (count == 1 ? "" : $"s");
            }

            count = (int)Math.Round(ts.TotalSeconds, 0);
            return $"{count} second" + (count == 1 ? "" : $"s");
        }

        public static string ToHumanString(this DateTime dateUtc, bool shorten=true)
        {
            DateTime date = dateUtc.ToLocalTime();
            string format = "yy-MM-dd HH:mm";
             // for date great than a day since, we don't care about time
            if (shorten && (DateTime.Now - date).TotalHours > 24)
                format = "yy-MM-dd";

            string shortened = date.ToString(format);
            return shortened;
        }

        public static string ToHumanString(this DateTime? dateUtc)
        {
            if (!dateUtc.HasValue)
                return string.Empty;

            return ToHumanString(dateUtc.Value);
        }
    }
}
