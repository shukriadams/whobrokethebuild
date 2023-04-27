namespace Wbtb.Core.Common
{
    public class AlertConfig
    {
        /// <summary>
        /// Id of contact plugin that will be used to contact group/user. Must impliment the IContactMethod interface.
        /// </summary>
        public string Plugin { get;set; }

        /// <summary>
        /// exposed identity of group/user in the system, f.ex, user's email address in an SMTP system
        /// </summary>
        public string Key { get; set; }

        public string RawJson { get; set; }
    }
}
