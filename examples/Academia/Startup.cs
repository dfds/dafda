// #define NOTIFY_INPROGRESS
// #define NOTIFY_POSTGRES

using Academia.Application;
using Academia.Domain;
using Academia.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Academia
{
    public static class Messaging
    {
    }

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
            services.AddSingleton<Stats>();
            services.AddTransient<StudentApplicationService>();
            // domain events are scoped so we can access them in TransactionalOutbox
            services.AddScoped<DomainEvents>();
            services.AddScoped<IDomainEvents>(provider => provider.GetRequiredService<DomainEvents>());

            // configure persistence (Postgres)
            var connectionString = Configuration["ACADEMIA_CONNECTION_STRING"];
            services.AddDbContext<SampleDbContext>(options => options.UseNpgsql(connectionString));
            services.AddTransient<IStudentRepository, RelationalStudentRepository>();

            // configure messaging
#if NOTIFY_INPROGRESS
            services.AddInProcessOutboxMessaging(Configuration);
#elif NOTIFY_POSTGRES
            services.AddPostgresNotifyOutboxMessaging(Configuration);
#else
            services.AddOutOfBandOutboxMessaging(Configuration);
#endif

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