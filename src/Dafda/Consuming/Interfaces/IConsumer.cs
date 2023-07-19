using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Dafda.Consuming.Interfaces
{
    public interface IConsumer
    {
        Task ConsumeAll(CancellationToken cancellationToken);

        Task ConsumeSingle(CancellationToken cancellationToken);
    }
}
