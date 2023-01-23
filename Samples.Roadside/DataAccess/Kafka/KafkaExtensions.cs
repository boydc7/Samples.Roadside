using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samples.Roadside.Contracts;
using Samples.Roadside.Models.Events;

namespace Samples.Roadside.DataAccess.Kafka;

public static class KafkaExtensions
{
    public static IServiceCollection AddKafkaEventProducer(this IServiceCollection serviceCollection)
        => serviceCollection.AddSingleton<IDispatchEventProducer, KafkaDispatchEventProducer>()
                            .AddSingleton<ILocationEventProducer, KafkaLocationEventProducer>()
                            .AddSingleton<ITransform<AssistantDispatched, AssistantDispatch>, AssistantDispatchedTransform>()
                            .AddSingleton<ITransform<AssistantLocationUpdate, AssistantLocation>, AssistantLocationUpdateTransform>();

    public static IServiceCollection AddKafkaDispatchEventConsumer(this IServiceCollection serviceCollection)
        => serviceCollection.AddSingleton<IDispatchEventConsumer, KafkaDispatchEventConsumer>()
                            .AddSingleton<ITransform<AssistantDispatched, AssistantDispatch>, AssistantDispatchedTransform>();
    
    public static IServiceCollection AddKafkaLocationEventConsumer(this IServiceCollection serviceCollection)
        => serviceCollection.AddSingleton<ILocationEventConsumer, KafkaLocationEventConsumer>()
                            .AddSingleton<ITransform<AssistantLocationUpdate, AssistantLocation>, AssistantLocationUpdateTransform>();
                            
    public static void LogKafkaMessage(this ILogger log, LogMessage logMessage)
        => log.Log(GetNetCoreLogLevel(logMessage),
                   "{LogMessage} :: via client [{KafkaClientInstance}]",
                   logMessage.Message,
                   logMessage.Name);

    public static void LogKafkaError(this ILogger log, Error kafkaError)
        => log.Log(LogLevel.Error,
                   "{ErrorMessage} :: errorCode [{KafkaErrorCode}], errorSource [{KafkaErrorSource}]",
                   kafkaError.ToString(),
                   kafkaError.Code,
                   kafkaError.IsLocalError
                       ? "local"
                       : "broker");

    public static LogLevel GetNetCoreLogLevel(this LogMessage kafkaLogMessage)
    {
        try
        {
            return (LogLevel)kafkaLogMessage.LevelAs(LogLevelType.MicrosoftExtensionsLogging);
        }
        catch
        {
            return LogLevel.Information;
        }
    }
}
