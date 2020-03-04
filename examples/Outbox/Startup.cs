using Dafda.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Outbox.Application;
using Outbox.Domain;
using Outbox.Infrastructure.Persistence;

namespace Outbox
{
    public class Startup
    {
        private const string StudentTopic = "test-topic";
        private const string StudentEnrolledMessageType = "student-enrolled";
        private const string StudentChangedEmailMessageType = "student-changed-email";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // configure main application
            services.AddSingleton<Stats>();
            services.AddTransient<StudentApplicationService>();
            // domain events are scoped so we can access them in TransactionalOutbox
            services.AddScoped<DomainEvents>();
            services.AddScoped<IDomainEvents>(provider => provider.GetRequiredService<DomainEvents>());

            // configure persistence (Postgres)
            var connectionString = Configuration["SAMPLE_DATABASE_CONNECTION_STRING"];
            services.AddDbContext<SampleDbContext>(options => options.UseNpgsql(connectionString));
            services.AddTransient<IStudentRepository, RelationalStudentRepository>();
            services.AddTransient<TransactionalOutbox>();

            // configure messaging: consumer
            services.AddConsumer(options =>
            {
                // kafka consumer settings
                options.WithConfigurationSource(Configuration);
                options.WithEnvironmentStyle("DEFAULT_KAFKA");
                options.WithEnvironmentStyle("SAMPLE_KAFKA");
                options.WithGroupId("foo");

                // register message handlers
                options.RegisterMessageHandler<StudentEnrolled, StudentEnrolledHandler>(StudentTopic, StudentEnrolledMessageType);
                options.RegisterMessageHandler<StudentChangedEmail, StudentChangedEmailHandler>(StudentTopic, StudentChangedEmailMessageType);
            });

            // configure the outbox pattern using Dafda
            services.AddOutbox(options =>
            {
                // register outgoing (through the outbox) messages
                options.Register<StudentEnrolled>(StudentTopic, StudentEnrolledMessageType, @event => @event.StudentId);
                options.Register<StudentChangedEmail>(StudentTopic, StudentChangedEmailMessageType, @event => @event.StudentId);

                // include outbox persistence
                options.WithOutboxEntryRepository<OutboxEntryRepository>();

                // no notifier configured
            });

            // no outbox producer configuration here

            // configure web api
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region configure web api application

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            #endregion
        }
    }
}