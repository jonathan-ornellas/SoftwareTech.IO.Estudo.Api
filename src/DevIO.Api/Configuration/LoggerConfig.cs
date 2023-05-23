using DevIO.Api.Extensions;
using Elmah.Io.Extensions.Logging;

namespace DevIO.Api.Configuration
{
    public static class LoggerConfig
    {
        public static IServiceCollection AddLoggingConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddElmahIo(o =>
            {
                o.ApiKey = "a7694087ff4e44199bbd389767c71592";
                o.LogId = new Guid("bc07a5e7-acdd-4198-bd5b-5da0cc1dabf6");
            });

            //services.AddLogging(builder =>
            //{
            //    builder.AddElmahIo(o =>
            //    {
            //        o.ApiKey = "a7694087ff4e44199bbd389767c71592";
            //        o.LogId = new Guid("bc07a5e7-acdd-4198-bd5b-5da0cc1dabf6");

            //    });

            //    builder.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);
            //});

            //services.AddHealthChecks()
            //    .AddElmahIoPublisher(options =>
            //    {
            //        options.ApiKey = "388dd3a277cb44c4aa128b5c899a3106";
            //        options.LogId = new Guid("c468b2b8-b35d-4f1a-849d-f47b60eef096");
            //        options.HeartbeatId = "API Fornecedores";

            //    })
                //.AddCheck("Produtos", new SqlServerHealthCheck(configuration.GetConnectionString("DefaultConnection")))
                //.AddSqlServer(configuration.GetConnectionString("DefaultConnection"), name: "BancoSQL");

            //services.AddHealthChecksUI()
            //    .AddSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));

            return services;
        }

        public static IApplicationBuilder UseLoggingConfiguration(this IApplicationBuilder app)
        {
            app.UseElmahIo();

            return app;
        }
    }
}