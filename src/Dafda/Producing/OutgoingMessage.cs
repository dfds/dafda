namespace Dafda.Producing
{
    public class OutgoingMessage
    {
        public OutgoingMessage(string topic, string key, string message)
        {
            Topic = topic;
            Key = key;
            Message = message;
        }

        public string Topic { get; }
        public string Key { get; }
        public string Message { get; }
    }
}