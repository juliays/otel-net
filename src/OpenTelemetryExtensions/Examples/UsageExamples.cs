using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetryExtensions.Extensions;

namespace OpenTelemetryExtensions.Examples
{
    public static class UsageExamples
    {
        public static void WebApiExample()
        {
            var builder = WebApplication.CreateBuilder(new string[] { });

            builder.AddTelemetry();

            builder.Services.AddControllers();

            var app = builder.Build();
            app.UseHttpsRedirection();
            app.MapControllers();
            app.Run();
        }

        public static async Task ConsoleAppExample()
        {
            var host = Host.CreateDefaultBuilder(new string[] { })
                .AddTelemetry()
                .ConfigureServices((context, services) =>
                {
                })
                .Build();

            await host.RunAsync();
        }
    }

    [FunctionsStartup(typeof(FunctionStartupExample))]
    public class FunctionStartupExample : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddTelemetry();
        }
    }
}
