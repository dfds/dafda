using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace InProcessOutbox.Infrastructure.Persistence
{
    public class OutboxUnitOfWorkFactory : IOutboxUnitOfWorkFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OutboxUnitOfWorkFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IOutboxUnitOfWork> Begin(CancellationToken cancellationToken)
        {
            var serviceScope = _serviceScopeFactory.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<SampleDbContext>();
            var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            return new OutboxUnitOfWork(serviceScope, dbContext, transaction);
        }

        private class OutboxUnitOfWork : IOutboxUnitOfWork
        {
            private readonly IServiceScope _serviceScope;
            private readonly SampleDbContext _dbContext;
            private readonly IDbContextTransaction _transaction;

            public OutboxUnitOfWork(IServiceScope serviceScope, SampleDbContext dbContext, IDbContextTransaction transaction)
            {
                _serviceScope = serviceScope;
                _dbContext = dbContext;
                _transaction = transaction;
            }

            public async Task<ICollection<OutboxEntry>> GetAllUnpublishedEntries(CancellationToken stoppingToken)
            {
                var entries = await _dbContext
                    .OutboxEntries
                    .Where(x => x.ProcessedUtc == null)
                    .ToListAsync(stoppingToken);

                entries.ForEach(message => _serviceScope.ServiceProvider.GetRequiredService<Stats>().Produce());

                return entries;
            }

            public async Task Commit(CancellationToken stoppingToken)
            {
                await _dbContext.SaveChangesAsync(stoppingToken);

                _transaction.Commit();
            }

            public void Dispose()
            {
                _transaction?.Dispose();
                _dbContext?.Dispose();
                _serviceScope?.Dispose();
            }
        }
    }
}