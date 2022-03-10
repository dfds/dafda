using Microsoft.Extensions.Hosting;

namespace Dafda.Configuration
{
    /// <summary>
    /// The different strategies for handling "catastrophic" failures in the consumer.
    /// </summary>
    public enum ConsumerFailureStrategy
    {
        /// <summary>
        /// The default failure strategy is to call <see cref="IHostApplicationLifetime.StopApplication"/>,
        /// which should gracefully shutdown the running application.
        /// </summary>
        Default,

        /// <summary>
        /// The consumer will be restarted.
        /// </summary>
        RestartConsumer,
    }
}