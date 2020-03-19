using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace DotnetKafka.Kafka
{
    public class KafkaProducer : IDisposable
    {
        private readonly ILogger<KafkaProducer> _logger;
        private readonly IProducer<Null, string> _producer;

        public KafkaProducer(ILogger<KafkaProducer> logger, ProducerConfig config)
        {
            _logger = logger;
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task ProduceAsync(string topic, string message)
        {
             await _producer.ProduceAsync(topic, new Message<Null, string> {Value = message});
             _logger.LogInformation($"Delivered message to {topic} => [{message}]");
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}