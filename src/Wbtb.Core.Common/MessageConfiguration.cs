namespace Wbtb.Core.Common
{
    public class MessageConfiguration
    {
        /// <summary>
        /// Id of contact plugin that will be used to contact group/user. Must impliment the IContactMethod interface.
        /// </summary>
        public string Plugin { get;set; }

        public string RawJson { get; set; }
    }
}
