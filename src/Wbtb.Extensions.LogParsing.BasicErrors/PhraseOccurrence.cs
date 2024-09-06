namespace Wbtb.Extensions.LogParsing.BasicErrors
{
    /// <summary>
    /// Defines a string phrase that appears in logs a number of times.
    /// </summary>
    internal class PhraseOccurrence
    {
        public string Phrase { get; set; }

        public int Count { get; set; }
    }
}
