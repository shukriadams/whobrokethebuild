using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public class SourceServer : IIdentifiable
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
        /// 
        /// </summary>
        public string KeyPrev { get; set; }

        /// <summary>
        /// Plugin used to communicate with server.
        /// </summary>
        public string Plugin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set;}

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Unique, immutable name of VC server, egs "git". Can be used to link to this type.
        /// </summary>
        public string ServerType { get; set; }

        /// <summary>
        /// credentials or authtoken to access server. encrypted.
        /// </summary>
        public ICredential AuthData {get;set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Addition key-value config specific to each plugin. These are defined in config.yml
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Config { get; set; }

        public SourceServer()
        { 
            this.Enable = true;
            this.Config = new List<KeyValuePair<string, object>>();
        }
    }
}
