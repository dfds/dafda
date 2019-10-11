using Dafda.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Infrastructure.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigurePersistence(this IServiceCollection services, string connectionString)
        {
            services
                .AddEntityFrameworkNpgsql()
                .AddDbContext<SampleDbContext>(options => options.UseNpgsql(connectionString));
        }
    }

    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // add other models

            modelBuilder.Entity<OutboxMessage>(cfg =>
            {
                cfg.ToTable("OutboxMessage", "outbox");
                cfg.HasKey(x => x.MessageId);
                cfg.Property(x => x.MessageId);
                cfg.Property(x => x.CorrelationId);
                cfg.Property(x => x.Topic);
                cfg.Property(x => x.Key);
                cfg.Property(x => x.Type);
                cfg.Property(x => x.Format);
                cfg.Property(x => x.Data);
                cfg.Property(x => x.OccurredOnUtc);
                cfg.Property(x => x.ProcessedUtc);
            });
        }
    }
}