using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Dapper;
using Npgsql;

namespace OutboxProcessor
{
    public class OutboxUnitOfWorkFactory : IOutboxUnitOfWorkFactory
    {
        private readonly string _connectionString;

        public OutboxUnitOfWorkFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IOutboxUnitOfWork> Begin(CancellationToken cancellationToken)
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            var transaction = await connection.BeginTransactionAsync(cancellationToken);

            return new OutboxUnitOfWork(connection, transaction);
        }

        private class OutboxUnitOfWork : IOutboxUnitOfWork
        {
            private readonly NpgsqlConnection _connection;
            private readonly NpgsqlTransaction _transaction;

            private IList<OutboxEntry> _entries;

            public OutboxUnitOfWork(NpgsqlConnection connection, NpgsqlTransaction transaction)
            {
                _connection = connection;
                _transaction = transaction;
            }

            private static string Q(string s) => '"' + s + '"';

            public async Task<ICollection<OutboxEntry>> GetAllUnpublishedEntries(CancellationToken stoppingToken)
            {
                var sql = $"select \"Id\" as \"MessageId\", \"Topic\", \"Key\", \"Payload\", \"OccurredUtc\", \"ProcessedUtc\" from \"_outbox\" " +
                    $"where {Q(nameof(OutboxEntry.ProcessedUtc))} is null " +
                    $"order by {Q(nameof(OutboxEntry.OccurredUtc))} asc;";

                var outboxMessages = await _connection.QueryAsync<OutboxEntry>(sql, _transaction);

                _entries = outboxMessages.ToImmutableList();

                return _entries;
            }

            public async Task Commit(CancellationToken stoppingToken)
            {
                if (_entries != null)
                {
                    var messagesToSave = _entries.Where(x => x.ProcessedUtc.HasValue);

                    foreach (var message in messagesToSave)
                    {
                        var sql = $"update \"_outbox\" set " +
                            $"{Q(nameof(OutboxEntry.ProcessedUtc))} = @processedUtc " +
                            $"where {Q("Id")} = @id;";

                        await _connection.ExecuteAsync(sql, new {id = message.MessageId, processedUtc = DateTime.UtcNow}, _transaction);
                    }
                }

                await _transaction.CommitAsync(stoppingToken);
            }

            public void Dispose()
            {
                _transaction?.Dispose();
                _connection?.Dispose();
            }
        }
    }
}