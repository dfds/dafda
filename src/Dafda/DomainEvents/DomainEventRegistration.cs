using System;

namespace Dafda.DomainEvents
{
    public class DomainEventRegistration
    {
        public Type HandlerInstanceType { get; set; }
        public Type MessageInstanceType { get; set; }
        public string Topic { get; set; }
        public string MessageType { get; set; }
    }
}