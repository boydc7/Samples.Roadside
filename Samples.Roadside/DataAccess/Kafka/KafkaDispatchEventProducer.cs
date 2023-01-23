using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Samples.Roadside.Configuration;
using Samples.Roadside.Contracts;
using Samples.Roadside.Models.Events;

namespace Samples.Roadside.DataAccess.Kafka;

public class KafkaDispatchEventProducer : IDispatchEventProducer
{
    private const string _topic = "roadside-assistant-dispatch";
    private readonly ILogger<KafkaDispatchEventProducer> _log;
    private readonly ITransform<AssistantDispatched, AssistantDispatch> _transform;
    private readonly IProducer<string, byte[]> _producer;

    public KafkaDispatchEventProducer(IConfiguration configuration, ILogger<KafkaDispatchEventProducer> log,
                                      ITransform<AssistantDispatched, AssistantDispatch> transform)
    {
        _log = log;
        _transform = transform;
        
        var bootstrapServers = configuration.GetConnectionString("Kafka") ?? throw new ArgumentNullException(nameof(configuration));

        _log.LogInformation("KafkaProducer bootstraps [{Bootstraps}]", bootstrapServers);
        
        _producer = KafkaProducerFactory.GetOrCreate(bootstrapServers, log, "roadside");
    }

    public async Task ProduceAsync(AssistantDispatched message)
    {
        var proto = _transform.Transform(message);

        var bytes = proto.ToByteArray();
        var key = Guid.NewGuid().ToString();

        _log.LogDebug("Producing AssistantDispatched message with key [{MessageId}] and length [{ByteLen}]", key, bytes.Length);
            
        await _producer.ProduceAsync(_topic,
                                     new Message<string, byte[]>
                                     {
                                         Key = key,
                                         Value = bytes
                                     },
                                     SampleShutdownCancellationSource.Instance.Token);
    }
}
