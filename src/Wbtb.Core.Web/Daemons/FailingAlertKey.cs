using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    public class FailingAlertKey
    {
        #region FIELDS

        private readonly MutationHelper _mutationHelper;

        #endregion

        #region CTORS

        public FailingAlertKey(MutationHelper mutationHelper) 
        {
            _mutationHelper = mutationHelper;
        }

        #endregion

        #region METHODS

        public string Get(Job job, Build incident)
        {
            string incidentMutation = _mutationHelper.GetBuildMutation(incident);
            return $"{incidentMutation}_{job.Key}_deltaAlert_{incident.Status}";
        }

        #endregion
    }
}
