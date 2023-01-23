using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Samples.Roadside.Contracts;
using Samples.Roadside.Models;
using Samples.Roadside.Models.Events;

namespace Samples.Roadside.DataAccess.Kafka;

public class KafkaLocationEventConsumer : ILocationEventConsumer, IAsyncDisposable
{
    private const string _topicName = "roadside-assistant-location";

    private bool _inShutdown;
    private readonly ILogger<KafkaLocationEventConsumer> _log;
    private readonly ITransform<AssistantLocationUpdate, AssistantLocation> _transform;
    private readonly IConsumer<string, byte[]> _consumer;
    private readonly TimeSpan _streamReadTimeout;

    public KafkaLocationEventConsumer(IConfiguration configuration, ILogger<KafkaLocationEventConsumer> log,
                                      ITransform<AssistantLocationUpdate, AssistantLocation> transform)
    {
        _log = log;
        _transform = transform;
        _streamReadTimeout = TimeSpan.FromSeconds(7);

        var bootstrapServers = configuration.GetConnectionString("Kafka") ?? throw new ArgumentNullException(nameof(configuration));

        _consumer = KafkaConsumerFactory.Instance.GetOrCreate(bootstrapServers, log, clientId: "roadside.location");
    }

    public IEnumerable<ConsumedEvent<AssistantLocationUpdate>> Consume()
    {
        try
        {
            _consumer.Subscribe(_topicName);

            do
            {
                var consumedEvent = KafkaConsume(_streamReadTimeout);

                if (consumedEvent == null)
                {
                    yield break;
                }

                var model = _transform.Transform(consumedEvent.Event);

                yield return new ConsumedEvent<AssistantLocationUpdate>
                             {
                                 Partition = consumedEvent.Partition,
                                 Offset = consumedEvent.Offset,
                                 Event = model
                             };
            } while (!_inShutdown);
        }
        finally
        {
            _consumer.Unsubscribe();
        }
    }

    private ConsumedEvent<AssistantLocation> KafkaConsume(TimeSpan timeout)
    {
        var consumed = _consumer.Consume(timeout);

        if (consumed?.Message?.Value == null || consumed.Message.Value.Length <= 0)
        {
            return null;
        }

        var proto = AssistantLocation.Parser.ParseFrom(consumed.Message.Value);

        _log.LogDebug("ConsumedEvent AssistantLocation message with key [{MessageId}] and length [{ByteLen}] for AssistantId [{AssistantId}], Lat/Long [{Latitude},{Longitude}]", 
                      consumed.Message.Key, consumed.Message.Value.Length,
                      proto.AssistantId, proto.Latitude, proto.Longitude);
        
        return new ConsumedEvent<AssistantLocation>
               {
                   Event = proto,
                   Partition = consumed.Partition.Value,
                   Offset = consumed.Offset.Value
               };
    }

    public async IAsyncEnumerable<T> AckConsumesAsync<T>(IAsyncEnumerable<T> messages)
        where T : IPartitionOffset
    {
        await foreach (var message in messages)
        {
            KafkaAckConsume(new TopicPartitionOffset(_topicName, message.Partition, message.Offset));

            yield return message;
        }
    }

    public void AckConsume(int partition, long offset)
        => KafkaAckConsume(new TopicPartitionOffset(_topicName, partition, offset));
    
    private void KafkaAckConsume(TopicPartitionOffset ack)
    {
        if (ack == null)
        {
            return;
        }

        _consumer.StoreOffset(ack);
    }

    private Task StopAsync()
    {
        _inShutdown = true;

        try
        {
            _consumer.Unsubscribe();
        }
        catch
        {
            // ignored
        }

        try
        {
            _consumer.Close();
        }
        catch
        {
            // ignored
        }

        try
        {
            _consumer.Dispose();
        }
        catch
        {
            // ignored
        }

        return Task.CompletedTask;
    }

    public void Dispose()
        => DisposeAsync().AsTask().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
    {
        await StopAsync();

        GC.SuppressFinalize(this);
    }
}
