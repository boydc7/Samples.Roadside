using System.Collections.Concurrent;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Samples.Roadside.DataAccess.Kafka;

internal class KafkaProducerFactory
{
    private static readonly ConcurrentDictionary<string, IProducer<string, byte[]>> _producerMap = new(StringComparer.OrdinalIgnoreCase);

    private KafkaProducerFactory() { }

    public static KafkaProducerFactory Instance { get; } = new();

    public static IProducer<string, byte[]> GetOrCreate(string bootstrapServers, ILogger log,
                                                        string clientId = null, int? messageTimeoutMs = null)
    {
        if (bootstrapServers.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(bootstrapServers));
        }

        return _producerMap.GetOrAdd(bootstrapServers,
                                     k => Create(k, log, clientId, messageTimeoutMs));
    }

    public static IProducer<string, byte[]> Create(string bootstrapServers, ILogger log,
                                                   string clientId = null, int? messageTimeoutMs = null)
    {
        var producerConfig = new ProducerConfig
                             {
                                 BootstrapServers = bootstrapServers,
                                 Acks = Acks.Leader,
                                 MessageTimeoutMs = messageTimeoutMs ?? 300_000
                             };

        if (!clientId.IsNullOrEmpty())
        {
            producerConfig.ClientId = clientId;
        }

        var producer = new ProducerBuilder<string, byte[]>(producerConfig).SetLogHandler((_, m) => log.LogKafkaMessage(m))
                                                                          .SetErrorHandler((_, x) => log.LogKafkaError(x))
                                                                          .Build();

        return producer;
    }
}
