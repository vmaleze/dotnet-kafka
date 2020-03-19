using System;
using System.Threading;
using Confluent.Kafka;
using DotnetKafka.Configuration;
using Microsoft.Extensions.Logging;

namespace DotnetKafka.Kafka
{
    public class KafkaConsumer : IDisposable
    {
        private readonly ILogger<KafkaConsumer> _logger;
        private readonly IConsumer<Ignore, string> _consumer;

        public KafkaConsumer(ILogger<KafkaConsumer> logger, ConsumerConfiguration consumer)
        {
            _logger = logger;
            _consumer = new ConsumerBuilder<Ignore, string>(consumer.Configuration).Build();
            _consumer.Subscribe(consumer.Topics);
        }

        public void Consume(CancellationToken ct)
        {
            try
            {
                var consumeResult = _consumer.Consume(ct);
                _logger.LogInformation($"Consuming message => {consumeResult.Message.Value}");
            }
            catch (ConsumeException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();
        }
    }
}