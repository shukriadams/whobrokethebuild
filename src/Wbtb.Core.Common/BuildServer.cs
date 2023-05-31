using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class BuildServer: IIdentifiable
    {
        /// <summary>
        /// Unique, immutable id in local infrastructure. Defined in config file for local setup. Will be used as parentID for child records. 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Semi-immutable id, will be defined in config
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// existing key in db. use when editing key from config. record will be edited so key in db matching keyprev is updated to new key in config
        /// </summary>
        public string KeyPrev { get; set; }

        /// <summary>
        /// Id of plugin to connect to build server
        /// </summary>
        public string Plugin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Unique, immutable name of CI server, egs "jenkins". Can be used to link to this type.
        /// </summary>
        public string ServerType { get; set; }

        /// <summary>
        /// Number of builds to import for all jobs on this build server. Can be overridden at the job level. If none set falls back to global default.
        /// </summary>
        public int? ImportCount {get; set; }

        /// <summary>
        /// Data used to generate a URL that can access the CI server (normally, username+password, or, an access token). This is encrypted and serialized as JSON
        /// </summary>
        public ICredential Credentials { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Addition key-value config specific to each plugin. These are defined in config.yml
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Config { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Job> Jobs { get; set; }

        public BuildServer()
        { 
            this.Enable = true;
            this.Jobs = new List<Job>();
            this.Config = new List<KeyValuePair<string, object>>();
        }
    }
}
