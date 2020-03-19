# Sample project for kafka and dotnet core

This project is a sample for using kafka with .net-core
It contains all the necessary tools to Produce and Consume messages

## TL;DR

Run kafka with `docker-compose up -d`  
Run `DotnetKafka.program`

POST a message to `http://localhost:8080/api/messages` :
```json
{
	"topic": "topic",
	"message": "test"
}
```

Message are consumed and logged by the dotnet app

## Kafka

I am using [wurstmeister](https://github.com/wurstmeister/kafka-docker) images as they are up to date and match my requirements.

To start kafka and zookeeper, launch `docker-compose up -d`

### Interact with Kafka

As I don't want to import shell scripts to interact with kafka, you can use the `start-kafka-shell.sh` script to interact with your cluster

* Consume a topic  
`$KAFKA_HOME/bin/kafka-console-consumer.sh --topic topic --bootstrap-server kafka:29092`

* Produce in a topic  
`$KAFKA_HOME/bin/kafka-console-producer.sh --topic topic --broker-list kafka:29092`

## Dotnet solution

To interact with kafka, we will be using the [Confluent.Kafka](https://github.com/confluentinc/confluent-kafka-dotnet) library

### Producer

Producing messages to kafka is the easy part.  
We simply need the url of our kafka cluster. 

First thing is to bind the Configuration of our producer :
```c#
public void ConfigureServices(IServiceCollection services)
{
    ...

    var producerConfig = new ProducerConfig();
    _configuration.Bind("Producer", producerConfig);
    services.AddSingleton(producerConfig);

    ...
}
```

Then create the associated Service :

```c#
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
```

And register it as a singleton :
```c#
public void ConfigureServices(IServiceCollection services)
{
    ...

    var producerConfig = new ProducerConfig();
    _configuration.Bind("Producer", producerConfig);
    services.AddSingleton(producerConfig);
    services.AddSingleton<KafkaProducer>();

    ...
}
```

To test it in our project, I simply created a `MessagesController` that takes a message and a topic in parameter and then calls my KafkaProducer.  
As the `KafkaProducer` is registered as a service, you can simply inject it and use it in all of your services.

### Consumer

The consumer is bit more tricky.  
A typical Kafka consumer application is centered around a consume loop, which repeatedly calls the Consume method to retrieve records one-by-one that have been efficiently pre-fetched by the consumer in background threads.  
Like so :
```c#
...

using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
{
    consumer.Subscribe(topics);

    while (!cancelled)
    {
        var consumeResult = consumer.Consume(cancellationToken);

        // handle consumed message.
        ...
    }

    consumer.Close();
}
```

In a main method, this is easy. But how do we implement this in a dotnet core solution and register our consumer as a service.

Let's begin by creating our `KafkaConsumer`
```c#
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
```

And register it as always:
```c#
public void ConfigureServices(IServiceCollection services)
{
    ...

    var consumerConfig = new ConsumerConfiguration();
    _configuration.Bind("Consumer", consumerConfig);
    services.AddSingleton(consumerConfig);

    ...
}
```

Now, we will use [background tasks](https://docs.microsoft.com/en-US/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1&tabs=visual-studio) 
to manage our service and use it:

```c#
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
```

As you can see in the code, we used a Task to run our consumer. This is required because there is no ConsumeAsync method.  
By doing this, we are creating a fake async consume method and we can still use ou HostedService.

## Conclusion
By either using the kafka console or the dotnet-app, you can now publish and consume messages from kafka.

## Links
Here are a list of links that I used to create this sample project

https://github.com/wurstmeister/kafka-docker  
https://www.kaaproject.org/kafka-docker  
https://docs.confluent.io/current/clients/dotnet.html  
https://github.com/srigumm/dotnetcore-kafka-integration  