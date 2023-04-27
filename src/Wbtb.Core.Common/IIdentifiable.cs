namespace Wbtb.Core.Common
{
    /// <summary>
    /// Defines an obnject with a key property that is defined in config.yml. Key must be immutable and unique
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// 
        /// </summary>
        string Key {get;set; }

        /// <summary>
        /// Existing key in db. use when editing key from config. record will be edited so key in db matching keyprev is updated to new key in config
        /// </summary>
        string KeyPrev { get; set; }
    }
}
