using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class BuildRevisionsRetrieveResult
    {
        public bool Success { get; set; }

        public string Result { get; set; }

        public IEnumerable<string> Revisions { get; set; }

        public BuildRevisionsRetrieveResult() 
        { 
            this.Revisions = new List<string>();
        }
    }
}
