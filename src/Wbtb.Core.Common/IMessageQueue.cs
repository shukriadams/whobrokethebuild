namespace Wbtb.Core.Common
{
    public interface IMessageQueue
    {
        string Add(object message);

        object Retrieve(string messageId);
    }
}
