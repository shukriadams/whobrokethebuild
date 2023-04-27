namespace Wbtb.Core.Common
{
    public class TokenCredential : ICredential
    {
        public virtual int ParentId { get; set; }

        public virtual string Token { get;set;}
    }
}
