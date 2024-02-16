namespace Wbtb.Core.Common
{
    /// <summary>
    /// Defines an interface for records which require write collision protection.
    /// </summary>
    public interface ISignature
    {
        /// <summary>
        /// Write-unique key for record, Used to prevent write collisions. A database record update will fail if the overwritten record has a different signature, and the signature is atomically updated as part of the update write.
        /// </summary>
        public string Signature { get;set;}
    }
}
