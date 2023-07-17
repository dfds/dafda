using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Dafda.Consuming.Interfaces
{
    /// <summary>
    /// Interfaces for hosted consumer
    /// </summary>
    public interface IConsumer
    {
        /// <summary>
        /// Consume all message
        /// </summary>
        Task ConsumeAll(CancellationToken cancellationToken);

        /// <summary>
        /// Consume single message
        /// </summary>
        Task ConsumeSingle(CancellationToken cancellationToken);
    }
}
