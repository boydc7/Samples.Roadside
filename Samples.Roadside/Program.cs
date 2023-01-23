using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Samples.Roadside.Contracts;

namespace Samples.Roadside;

internal static class Program
{
    internal static async Task Main()
    {
        var urls = BuildConfiguration.GetValue("SamplesRoadside:httphosturl", $"http://*:{8082}");

        var port = urls.LastRightPart(':').ToInt(8082);

        var builder = new HostBuilder().UseContentRoot(Directory.GetCurrentDirectory())
                                       .ConfigureHostConfiguration(b => b.AddConfiguration(BuildConfiguration))
                                       .ConfigureAppConfiguration((_, conf) => conf.AddConfiguration(BuildConfiguration))
                                       .ConfigureLogging((x, b) => b.AddConfiguration(x.Configuration.GetSection("Logging"))
                                                                    .AddSimpleConsole(o =>
                                                                                      {
                                                                                          o.SingleLine = true;
                                                                                          o.TimestampFormat = "HH:mm:ss.fff ";
                                                                                      }))
                                       .ConfigureWebHost(whb => whb.UseShutdownTimeout(TimeSpan.FromSeconds(15))
                                                                   .UseKestrel(ko =>
                                                                               {
                                                                                   ko.Listen(IPAddress.Any, port, l => l.Protocols = HttpProtocols.Http1AndHttp2);
                                                                                   ko.Limits.MaxRequestBodySize = 2000 * 1024;
                                                                                   ko.AllowSynchronousIO = false;
                                                                               })
                                                                   .UseStartup<SampleStartup>())
                                       .UseConsoleLifetime();

        var host = builder.Build();

        // Migrate/setup data stores
        var migrated = await host.Services.MigrateDataStoresAsync();
        
        // Create some seed data if we migrated anything
        if (migrated)
        {
            var demoDataService = host.Services.GetRequiredService<IDemoDataService>();
#pragma warning disable 4014
            await demoDataService.CreateDemoDataAsync();
#pragma warning restore 4014
        }
        
        await host.RunAsync();
    }

    private static IConfiguration BuildConfiguration { get; } = new ConfigurationBuilder().AddJsonFile("appsettings.Global.json", true)
                                                                                          .AddJsonFile("appsettings.json", true)
                                                                                          .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                                                                                          .AddEnvironmentVariables("ROADSIDE_")
                                                                                          .Build();
}
