using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Wbtb.Extensions.BuildServer.Jenkins
{
    /// <summary>
    /// tries to sort of an array number-like strings
    /// </summary>
    public class NumericStringComparer : IComparer<string>
    {
        public int Compare(string first, string second)
        {
            Regex regex = new Regex(@"^(\d+)");

            Match firstLookup = regex.Match(first);
            Match secondLookup = regex.Match(second);

            if (firstLookup.Success && secondLookup.Success)
                return int.Parse(firstLookup.Groups[1].Value).CompareTo(int.Parse(secondLookup.Groups[1].Value));

            // do regular string compare
            return first.CompareTo(second);
        }
    }
}
