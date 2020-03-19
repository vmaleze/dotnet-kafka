using Confluent.Kafka;
using DotnetKafka.Configuration;
using DotnetKafka.Events;
using DotnetKafka.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetKafka
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            services.AddHostedService<EventManager>();
            
            var producerConfig = new ProducerConfig();
            _configuration.Bind("Producer", producerConfig);
            services.AddSingleton(producerConfig);
            services.AddSingleton<KafkaProducer>();

            var consumerConfig = new ConsumerConfiguration();
            _configuration.Bind("Consumer", consumerConfig);
            services.AddSingleton(consumerConfig);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }
}
