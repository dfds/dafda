using System;
using Dafda.Configuration;
using Dafda.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.Application;
using Sample.Infrastructure.Persistence;

namespace Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // configure main application
            // services.AddHostedService<MainWorker>();

            services.AddSingleton<Stats>();

            // configure persistence (PostgreSQL)
            services.ConfigurePersistence(Configuration["SAMPLE_DATABASE_CONNECTION_STRING"]);

            services.AddHttpContextAccessor();

            services.AddTransient<Transactional>();
            services.AddScoped<DomainEvents>();
            services.AddTransient<ApplicationService>();

            // configure messaging: consumer
            services.AddConsumer(options =>
            {
                // configuration settings
                options.WithConfigurationSource(Configuration);
                options.WithEnvironmentStyle("DEFAULT_KAFKA");
                options.WithEnvironmentStyle("SAMPLE_KAFKA");
                options.WithGroupId("foo");

                // register message handlers
                options.RegisterMessageHandler<TestEvent, TestHandler>("test-topic", "test-event");
            });

            // configure ANOTHER messaging: consumer
            services.AddConsumer(options =>
            {
                // configuration settings
                options.WithConfigurationSource(Configuration);
                options.WithEnvironmentStyle("DEFAULT_KAFKA");
                options.WithEnvironmentStyle("SAMPLE_KAFKA");
                options.WithGroupId("bar");

                // register message handlers
                options.RegisterMessageHandler<TestEvent, AnotherTestHandler>("test-topic", "test-event");
            });

            var outboxNotification = new OutboxNotification(TimeSpan.FromSeconds(5));

            services.AddSingleton(provider => outboxNotification); // register to dispose

            // configure messaging: producer
            services.AddOutbox(options =>
            {
                // register outgoing messages (includes outbox messages)
                options.Register<TestEvent>("test-topic", "test-event", @event => @event.AggregateId);

                // include outbox persistence
                options.WithOutboxEntryRepository<OutboxEntryRepository>();
                options.WithNotifier(outboxNotification);
            });

            services.AddOutboxProducer(options =>
            {
                // configuration settings
                options.WithConfigurationSource(Configuration);
                options.WithEnvironmentStyle("DEFAULT_KAFKA");
                options.WithEnvironmentStyle("SAMPLE_KAFKA");

                // include outbox (polling publisher)
                options.WithUnitOfWorkFactory<OutboxUnitOfWorkFactory>();
                options.WithListener(outboxNotification);
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}