namespace Wbtb.Core.Common
{
    public enum Blame : int
    {
        // Everyone is a suspect
        Suspect = 0,

        /// <summary>
        /// Caught red-handed
        /// </summary>
        Guilty = 1,

        /// <summary>
        /// They got away this time.
        /// </summary>
        Innocent = 2
    }
}
