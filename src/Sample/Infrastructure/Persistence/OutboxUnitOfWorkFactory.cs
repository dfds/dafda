using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Infrastructure.Persistence
{
    public class OutboxUnitOfWorkFactory : IOutboxUnitOfWorkFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OutboxUnitOfWorkFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public IOutboxUnitOfWork Begin()
        {
            var serviceScope = _serviceScopeFactory.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<SampleDbContext>();
            var transaction = dbContext.Database.BeginTransaction();

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

            public async Task<ICollection<OutboxMessage>> GetAllUnpublishedMessages(CancellationToken stoppingToken)
            {
                var outboxMessages = await _dbContext
                    .OutboxMessages
                    .Where(x => x.ProcessedUtc == null)
                    .ToListAsync(stoppingToken);

                outboxMessages.ForEach(message => _serviceScope.ServiceProvider.GetRequiredService<Stats>().Produce());

                return outboxMessages;
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