namespace Wbtb.Core.Common
{
    public class StoreItem : IIdentifiable
    {
        public string Id { get; set; }

        public string Key { get; set; }

        public string Plugin { get; set; }

        public string KeyPrev { get; set; }

        public string Content { get; set; }
    }
}
