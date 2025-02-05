namespace Wbtb.Extensions.BuildServer.JenkinsSandbox
{
    /// <summary>
    /// JSON carrier for Jenkins API data
    /// </summary>
    internal class RawBuild
    {
        public string number { get; set; }

        public string result { get; set; }

        public string builtOn { get; set; }

        public string timestamp { get; set; }

        public string duration { get; set; }

        public string dev_isLatest { get; set; }
    }
}
