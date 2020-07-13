using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Dafda.Tests.TestDoubles
{
    public class DummyApplicationLifetime : IHostApplicationLifetime
    {
        public void StopApplication()
        {
        }

        public CancellationToken ApplicationStarted { get; }
        public CancellationToken ApplicationStopping { get; }
        public CancellationToken ApplicationStopped { get; }
    }
}