using System.Collections.Concurrent;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Samples.Roadside.DataAccess.Kafka;

internal class KafkaConsumerFactory
{
    private readonly ConcurrentDictionary<string, IConsumer<string, byte[]>> _consumerMap = new(StringComparer.OrdinalIgnoreCase);

    private KafkaConsumerFactory() { }

    public static KafkaConsumerFactory Instance { get; } = new();

    public IConsumer<string, byte[]> GetOrCreate(string bootstrapServers,
                                                 ILogger log,
                                                 string groupId = null,
                                                 string clientId = null)
    {
        if (bootstrapServers.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(bootstrapServers));
        }

        if (groupId.IsNullOrEmpty())
        {
            groupId = "roadside";
        }

        var consumerKey = string.Concat(groupId, "|", bootstrapServers, "|", clientId);

        return _consumerMap.GetOrAdd(consumerKey,
                                     k => Create(bootstrapServers, log, groupId, clientId));
    }

    public IConsumer<string, byte[]> Create(string bootstrapServers, ILogger log, string groupId, string clientId = null)
    {
        var producerConfig = new ConsumerConfig
                             {
                                 BootstrapServers = bootstrapServers,
                                 GroupId = groupId,
                                 Acks = Acks.Leader,
                                 EnableAutoCommit = true,
                                 EnableAutoOffsetStore = false,
                                 AutoOffsetReset = AutoOffsetReset.Earliest
                             };

#if DEBUG || LOCAL
        producerConfig.MaxPollIntervalMs = 86400000;
#endif

        if (!clientId.IsNullOrEmpty())
        {
            producerConfig.ClientId = clientId;
        }

        var consumer = new ConsumerBuilder<string, byte[]>(producerConfig).SetLogHandler((_, m) => log.LogKafkaMessage(m))
                                                                          .SetErrorHandler((_, x) => log.LogKafkaError(x))
                                                                          .Build();

        return consumer;
    }
}
