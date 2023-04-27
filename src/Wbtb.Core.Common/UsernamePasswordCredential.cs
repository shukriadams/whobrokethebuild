namespace Wbtb.Core.Common
{
    public class UsernamePasswordCredential : ICredential
    {
        public virtual int ParentId { get;set; }

        public virtual string Username { get; set; }

        public virtual string Password { get; set; }
    }
}
