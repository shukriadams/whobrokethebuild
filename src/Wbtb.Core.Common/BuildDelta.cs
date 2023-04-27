namespace Wbtb.Core.Common
{
    public enum BuildDelta
    {
        /// <summary>
        /// Default value
        /// </summary>
        Unprocessed=0,

        /// <summary>
        /// Previous build passed, passing continues.
        /// </summary>
        Pass=1,

        /// <summary>
        /// Previous build was not passing, but now is
        /// </summary>
        Restore=2,

        /// <summary>
        /// Previous build was passing or unknown, and is now broken
        /// </summary>
        Broke=3,

        /// <summary>
        /// Previous build was broke, and is still broken
        /// </summary>
        ContinuedBreak=4,

        /// <summary>
        /// Previous status is ignored, build is now in an unknown state (aborted, hanging, etc)
        /// </summary>
        Unknown=5
    }
}
