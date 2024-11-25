using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace SteeltoeRedisCacheSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Read Redis configuration from appsettings.json
            var redisHost = Configuration["Redis:Host"];
            var redisPort = Configuration["Redis:Port"];
            var redisInstanceName = Configuration["Redis:InstanceName"];

            // Add Redis ConnectionMultiplexer
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisOptions = ConfigurationOptions.Parse($"{redisHost}:{redisPort}");
                return ConnectionMultiplexer.Connect(redisOptions);
            });

            // Add Distributed Cache using Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = $"{redisHost}:{redisPort}";
                options.InstanceName = redisInstanceName;
            });

            // Add Controllers
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
