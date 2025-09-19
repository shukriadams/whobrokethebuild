namespace Wbtb.Core
{
    public class Response<T>
    {
        public string Error { get; set; }

        public T Value { get; set; }
    }
}
