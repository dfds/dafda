namespace Dafda.Consuming.MessageFilters
{
    /// <summary>
    /// Filter for messaging prior to calling consumer.
    /// Exposes Can Accept Message handler.
    /// </summary>
    public abstract class MessageFilter
    {
        /// <summary>
        /// Default message filter if one is not specified.
        /// </summary>
        public static MessageFilter Default = new DefaultMessageFilter();

        /// <summary>
        /// Overridable Can Accept Message flag.
        /// </summary>
        public abstract bool CanAcceptMessage(MessageResult result);

        private class DefaultMessageFilter : MessageFilter
        {
            public override bool CanAcceptMessage(MessageResult result)
            {
                // allow all messages to be processed
                return true;
            }
        }
    }
}
