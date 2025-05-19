using System.Linq;
using Wbtb.Core.Common;

namespace Wbtb.Core
{
    public class JobGroupLogic
    {
        #region PROPERTIES

        private readonly Configuration _config;
        private readonly SimpleDI _di;
        #endregion

        #region CTORS

        public JobGroupLogic(Configuration config) 
        {
            _config = config;
            _di = new SimpleDI();
        }

        #endregion

        #region METHODS

        public JobGroupStatusResponse GetStatus(string jobGroupKey)
        {
            JobGroup jobGroup = _config.JobGroups.FirstOrDefault(j => j.Key == jobGroupKey);
            if (jobGroup == null)
                return new JobGroupStatusResponse 
                {
                    Message = $"JobGroup {jobGroupKey} does not exist"
                };

            PluginProvider pluginProvider = _di.Resolve<PluginProvider>();
            IDataPlugin dataLayer = pluginProvider.GetFirstForInterface<IDataPlugin>();

            if (jobGroup.Behaviour == JobGroupBehaviour.FailIfAnyFails)
            {
                bool passing = true;
                foreach (string jobKey in jobGroup.Jobs)
                {
                    Job job = dataLayer.GetJobByKey(jobKey);
                    if (job == null)
                        return new JobGroupStatusResponse
                        {
                            Message = $"JobGroup {jobGroupKey} failed, expected job {jobKey} not found in data store."
                        };

                    Build latestBuildInJob = dataLayer.GetLatestPassOrFailBuildByJob(job);

                    // no build in job, ignore it
                    if (latestBuildInJob == null)
                        continue;

                    if (latestBuildInJob.Status != BuildStatus.Passed)
                        passing = false;
                }

                return new JobGroupStatusResponse
                { 
                    Success = true,
                    Status = passing? JobGroupStatus.Passed : JobGroupStatus.Failed
                };
            }
            else 
            {
                return new JobGroupStatusResponse
                {
                    Message = $"JobGroup {jobGroupKey} defines behaviour {jobGroup.Behaviour} that is not currently supported."
                };
            }

            return null;            
        }

        #endregion
    }
}
