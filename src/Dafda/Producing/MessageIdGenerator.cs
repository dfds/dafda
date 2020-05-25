using System;

namespace Dafda.Producing
{
    /// <summary>
    /// Inherit from this class and implement the <see cref="NextMessageId"/>,
    /// in order to generate unique message identifiers.
    /// </summary>
    public abstract class MessageIdGenerator
    {
        internal static readonly MessageIdGenerator Default = new DefaultMessageIdGenerator();

        /// <summary>
        /// Generate the next message identifier.
        /// </summary>
        /// <returns>A unique message identifier</returns>
        public abstract string NextMessageId();

        private class DefaultMessageIdGenerator : MessageIdGenerator
        {
            public override string NextMessageId()
            {
                return Guid.NewGuid().ToString();
            }
        }
    }
}