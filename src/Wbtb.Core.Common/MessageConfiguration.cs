namespace Wbtb.Core.Common
{
    public class MessageConfiguration
    {
        /// <summary>
        /// Id of contact plugin that will be used to contact group/user. Must impliment the IContactMethod interface.
        /// </summary>
        public string Plugin { get;set; }

        /// <summary>
        /// String from core YML config, converted to JSON. Will be parsed to plugin where it can be parsed internally in a format defined
        /// by that plugin.
        /// </summary>
        public string RawJson { get; set; }
    }
}
