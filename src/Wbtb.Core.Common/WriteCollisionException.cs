using System;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Thrown when record update detects record has already been changed.
    /// </summary>
    public class WriteCollisionException : Exception
    {
        public WriteCollisionException(string recordTytpe, string recordId) : base($"Update of {recordTytpe} id {recordId} failed, record already changed.")
        {

        }
    }
}
