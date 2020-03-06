using Dafda.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Infrastructure.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigurePersistence(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<SampleDbContext>(options => options.UseNpgsql(connectionString));
        }
    }

    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<OutboxEntry> OutboxEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // add other models

            modelBuilder.Entity<OutboxEntry>(cfg =>
            {
                cfg.ToTable("_outbox");
                cfg.HasKey(x => x.MessageId);
                cfg.Property(x => x.MessageId).HasColumnName("Id");
                cfg.Property(x => x.Topic);
                cfg.Property(x => x.Key);
                cfg.Property(x => x.Payload);
                cfg.Property(x => x.OccurredUtc);
                cfg.Property(x => x.ProcessedUtc);
            });
        }
    }
}