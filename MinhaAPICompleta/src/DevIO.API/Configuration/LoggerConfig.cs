using System;
using DevIO.API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevIO.API.Configuration
{
    public static class LoggerConfig
    {
        public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddElmahIo(o =>
            {
                o.ApiKey = "50f4cf119657465f9e166b0768bd3b35";
                o.LogId = new Guid("fa4772d9-8632-42cc-a70e-c031fb2ed503");
            });
            services.AddHealthChecks()
               .AddElmahIoPublisher(options =>
               {
                   options.ApiKey = "50f4cf119657465f9e166b0768bd3b35";
                   options.LogId = new Guid("fa4772d9-8632-42cc-a70e-c031fb2ed503");
                   options.HeartbeatId = "API Fornecedores";

               })
                .AddCheck("Produtos", new SqlServerHealthCheck(configuration.GetConnectionString("DefaultConnection")))
                .AddSqlServer(configuration.GetConnectionString("DefaultConnection"), name: "BancoSQL");

            services.AddHealthChecksUI()
                .AddSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));

            return services;

            /*
            services.AddLogging(builder =>
            {
                builder.AddElmahIo(o =>
                {
                    o.ApiKey = "50f4cf119657465f9e166b0768bd3b35";
                    o.LogId = new Guid("fa4772d9-8632-42cc-a70e-c031fb2ed503");
                });
                builder.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);
            });
            */
        }
        public static IApplicationBuilder UseLoggingConfiguration(this IApplicationBuilder app)
        {
            app.UseElmahIo();
            return app;
        }
    }
}
