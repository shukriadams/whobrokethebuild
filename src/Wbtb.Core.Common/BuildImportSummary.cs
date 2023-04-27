using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class BuildImportSummary
    {
        /// <summary>
        /// Ids of builds that were created as records. Status varies
        /// </summary>
        public IList<Build> Created { get; set; }

        /// <summary>
        /// Builds done
        /// </summary>
        public IList<Build> Ended { get;set;}

        public BuildImportSummary()
        {
            this.Created = new List<Build>();
            this.Ended = new List<Build>();
        }
    }
}
