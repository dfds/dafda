using System.Diagnostics;
using Academia.Domain;
using Dafda.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Academia.Infrastructure.Persistence
{
    [DebuggerDisplay("{" + nameof(ContextId) + "}")]
    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<OutboxEntry> OutboxEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>(cfg =>
            {
                cfg.ToTable("student");
                cfg.HasKey(x => x.Id);
                cfg.Property(x => x.Id).HasColumnName("id");
                cfg.Property(x => x.Name).HasColumnName("name");
                cfg.Property(x => x.Email).HasColumnName("email");
                cfg.Property(x => x.Address).HasColumnName("address");
            });

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