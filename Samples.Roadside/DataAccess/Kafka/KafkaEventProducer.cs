using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Samples.Roadside.Configuration;
using Samples.Roadside.Contracts;
using Samples.Roadside.Models.Events;

namespace Samples.Roadside.DataAccess.Kafka;

public class KafkaLocationEventProducer : ILocationEventProducer
{
    private const string _topicName = "roadside-assistant-location";
    private readonly ILogger<KafkaLocationEventProducer> _log;
    private readonly ITransform<AssistantLocationUpdate, AssistantLocation> _transform;
    private readonly IProducer<string, byte[]> _producer;

    public KafkaLocationEventProducer(IConfiguration configuration, ILogger<KafkaLocationEventProducer> log,
                                      ITransform<AssistantLocationUpdate, AssistantLocation> transform)
    {
        _log = log;
        _transform = transform;
        var bootstrapServers = configuration.GetConnectionString("Kafka") ?? throw new ArgumentNullException(nameof(configuration));

        _producer = KafkaProducerFactory.GetOrCreate(bootstrapServers, log, "roadside");
    }

    public async Task ProduceAsync(AssistantLocationUpdate message)
    {
        var proto = _transform.Transform(message);

        var bytes = proto.ToByteArray();
        var key = Guid.NewGuid().ToString();

        _log.LogDebug("Producing AssistantLocationUpdate message with key [{MessageId}] and length [{ByteLen}]", key, bytes.Length);
            
        await _producer.ProduceAsync(_topicName,
                                     new Message<string, byte[]>
                                     {
                                         Key = key,
                                         Value = bytes
                                     },
                                     SampleShutdownCancellationSource.Instance.Token);
    }
}
