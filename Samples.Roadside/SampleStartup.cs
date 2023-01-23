using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Samples.Roadside.Configuration;
using Samples.Roadside.Contracts;
using Samples.Roadside.DataAccess.Elastic;
using Samples.Roadside.DataAccess.Kafka;
using Samples.Roadside.DataAccess.Sql;
using Samples.Roadside.Middleware;
using Samples.Roadside.Services;

namespace Samples.Roadside;

internal class SampleStartup
{
    public void Configure(IApplicationBuilder app)
    {
        app.RegisterOnApplicationStopping(() =>
                                          {
                                              var logger = app.ApplicationServices.GetRequiredService<ILogger<SampleStartup>>();

                                              SampleShutdownCancellationSource.Instance.TryCancel();

                                              logger.LogInformation("*** Shutdown initiated, stopping services...");
                                          })
           .RegisterOnApplicationStopped(() =>
                                         {
                                             var logger = app.ApplicationServices.GetRequiredService<ILogger<SampleStartup>>();

                                             logger.LogInformation("*** Shutdown completed, exiting...");
                                         })
           .RegisterOnApplicationStarted(() =>
                                         {
                                             var logger = app.ApplicationServices.GetRequiredService<ILogger<SampleStartup>>();

                                             logger.LogInformation("SampleRoadside service is ready for requests");
                                         })
           .UseRouting()
           .UseMiddleware<SampleLogMiddleware>()
           .UseHealthChecks("/ping")
           .UseEndpoints(r => { r.MapControllers(); })
           .UseSwagger()
           .UseSwaggerUI(o =>
                         {
                             o.SwaggerEndpoint("/swagger/roadside/swagger.json", "Roadside API");
                             o.RoutePrefix = string.Empty;
                         });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHealthChecks()
                .Add(new HealthCheckRegistration("SampleRoadsideService", ServicePingHealthCheck.Instance, HealthStatus.Unhealthy,
                                                 Enumerable.Empty<string>(), TimeSpan.FromSeconds(7)));

        services.AddControllers()
                .AddJsonOptions(x =>
                                {
                                    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                                    x.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                                    x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                                    x.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                                    x.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                                    x.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                                    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                                })
                .AddMvcOptions(o =>
                               {
                                   o.Filters.Add(new ModelAttributeValidationFilter());
                                   o.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "The field is required.");
                                   o.MaxModelValidationErrors = 10;
                               });

        services.AddSwaggerGen(c =>
                               {
                                   c.SwaggerDoc("roadside", new OpenApiInfo
                                                            {
                                                                Version = "v1",
                                                                Title = "RoadsideAssistance",
                                                                Description = "Simple service that provides ability for customers to find and dispatch roadside assistance providers to their location"
                                                            });

                                   foreach (var xmlFile in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly))
                                   {
                                       c.IncludeXmlComments(xmlFile);
                                   }
                               });

        services.AddElasticAssistantLocationTracking()
                .AddElasticAssistantFinderServices()
                .AddElasticDispatchUpdating()
                .AddKafkaEventProducer()
                .AddKafkaLocationEventConsumer()
                .AddKafkaDispatchEventConsumer()
                .AddSqlServerDispatchServices()
                .AddSqlServerModelServices();

        // Internal services
        services.AddSingleton<IRoadsideAssistanceService, RoadsideAssistanceService>()
                .AddHostedService<AssistantDispatchHostedService>()
                .AddHostedService<AssistantLocationUpdateHostedService>()
                .AddSingleton<IDemoDataService, SampleDemoDataService>();
    }
}
