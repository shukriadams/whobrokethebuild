using System;

namespace Wbtb.Core.Common
{
    public static class DateTimeExtensions
    {
        public static string ToISOShort(this DateTime date)
        {
            string iso = date.ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                .Replace("T", " ");

            // clip off seconds
            return iso.Substring(0, iso.Length -3);

        }
    }
}
