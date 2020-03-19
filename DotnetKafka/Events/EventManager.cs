using System.Threading;
using System.Threading.Tasks;
using DotnetKafka.Configuration;
using DotnetKafka.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotnetKafka.Events
{
    public class EventManager : BackgroundService
    {
        private readonly ILogger<EventManager> _logger;
        private readonly KafkaConsumer _consumer;
        private readonly ConsumerConfiguration _configuration;

        public EventManager(ILoggerFactory loggerFactory, ConsumerConfiguration consumerConfiguration)
        {
            _logger = loggerFactory.CreateLogger<EventManager>();
            _configuration = consumerConfiguration;
            _consumer = new KafkaConsumer(loggerFactory.CreateLogger<KafkaConsumer>(), _configuration);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Starting EventManager service. Consuming to the following topics [{string.Join(", ", _configuration.Topics)}]");

            await Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _consumer.Consume(stoppingToken);
                }
            }, stoppingToken);
        }
    }
}