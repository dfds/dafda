using System;

namespace Dafda.Consuming
{
    internal class PoisonAwareIncomingMessageFactory : IIncomingMessageFactory
    {
        private readonly IIncomingMessageFactory _innerIncomingMessageFactory;

        public PoisonAwareIncomingMessageFactory(IIncomingMessageFactory innerIncomingMessageFactory)
        {
            _innerIncomingMessageFactory = innerIncomingMessageFactory;
        }

        public TransportLevelMessage Create(string rawMessage)
        {
            try
            {
                return _innerIncomingMessageFactory.Create(rawMessage);
            }
            catch(Exception ex)
            {
                //TODO: _logger.LogWarning(ex, "Exception thrown when creating transport level message.");
                return new TransportLevelMessage(new Metadata() { Type = TransportLevelPoisonMessage.Type }, (t) => new TransportLevelPoisonMessage(rawMessage, ex));
            }
        }
    }
}
