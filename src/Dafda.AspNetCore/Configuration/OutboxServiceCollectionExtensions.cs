using System;
using Dafda.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public static class OutboxServiceCollectionExtensions
    {
        public static IServiceCollection AddOutbox(this IServiceCollection services, Action<IOutboxOptions> config)
        {
            var registry = new OutboxRegistry();
            var configuration = new OutboxOptions(services, registry);
            config?.Invoke(configuration);

            services.AddTransient<IOutbox>(provider =>
            {
                var repository = provider.GetRequiredService<IOutboxMessageRepository>();
                return new OutboxMessageCollector(registry, repository);
            });

            return services;
        }
    }
}