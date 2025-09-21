namespace Wbtb.Extensions.LogParsing.SimpleRegex
{
    /// <summary>
    /// Locally defined wrapper for expected plugin config
    /// </summary>
    public class SimpleRegexConfig
    {
        public string Regex { get; set; }

        public string SectionDelimiter { get; set; }

        public IList<Describe> Describes { get; set; } = new List<Describe>();
    }
}
