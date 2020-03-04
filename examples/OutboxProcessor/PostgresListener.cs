using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Npgsql;
using Serilog;

namespace OutboxProcessor
{
    public class PostgresListener : IOutboxListener, IDisposable
    {
        private readonly Lazy<NpgsqlConnection> _connection;
        private readonly TimeSpan _timeout;
        private readonly string _channel;

        public PostgresListener(string connectionString, string channel, TimeSpan timeout)
        {
            _timeout = timeout;
            _channel = channel;
            _connection = new Lazy<NpgsqlConnection>(() => ConnectionFactory(connectionString, _channel), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static NpgsqlConnection ConnectionFactory(string connectionString, string channel)
        {
            var conn = new NpgsqlConnection(connectionString);

            conn.Open();

            conn.Notification += OnNotification;

            using (var command = conn.CreateCommand())
            {
                command.CommandText = $"LISTEN {channel};";
                command.ExecuteNonQuery();
            }

            return conn;
        }

        private static void OnNotification(object sender, NpgsqlNotificationEventArgs eventArgs)
        {
            Log.Debug("Asynchronous notification {Channel} with payload {Payload} received from server process with PID {PID}.", eventArgs.Channel, eventArgs.Payload, eventArgs.PID);
        }

        public async Task<bool> Wait(CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var timedOut = false;

                cts.CancelAfter(_timeout);

                Log.Information("LISTENING on {Channel}", _channel);

                try
                {
                    await _connection.Value.WaitAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    timedOut = true;
                }

                Log.Debug(timedOut ? "Timeout" : "Notified");

                return timedOut;
            }
        }

        public void Dispose()
        {
            if (_connection.IsValueCreated)
            {
                _connection.Value.Dispose();
            }
        }
    }
}