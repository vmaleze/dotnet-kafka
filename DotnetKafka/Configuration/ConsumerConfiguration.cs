using System.Collections.Generic;
using Confluent.Kafka;

namespace DotnetKafka.Configuration
{
    public class ConsumerConfiguration
    {
        public IEnumerable<string> Topics { get; set; }
        
        public ConsumerConfig Configuration { get; set; }
    }
}