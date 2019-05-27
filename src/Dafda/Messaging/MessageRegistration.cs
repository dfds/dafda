using System;

namespace Dafda.Messaging
{
    public class MessageRegistration
    {
        public Type HandlerInstanceType { get; set; }
        public Type MessageInstanceType { get; set; }
        public string Topic { get; set; }
        public string MessageType { get; set; }
    }
}