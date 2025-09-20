namespace Wbtb.Core.Common
{
    public struct Response<T>
    {
        public string Error { get; set; }

        public T Value { get; set; }
    }
}
