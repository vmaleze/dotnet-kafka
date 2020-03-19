using System.Threading.Tasks;
using DotnetKafka.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace DotnetKafka.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly KafkaProducer _producer;

        public ValuesController(KafkaProducer producer)
        {
            _producer = producer;
        }
        
        [HttpPost]
        public async Task Post([FromBody] KafkaMessage kafkaMessage)
        {
            await _producer.ProduceAsync(kafkaMessage.Topic, kafkaMessage.Message);
        }
    }
}
