using Confluent.Kafka;
using Confluent.Kafka.Admin;

#region Create topic if not exist
using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BROKERS") }).Build();

var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
if (!metadata.Topics.Any(t => t.Topic == "message-topic"))
{
    var topicSpecification = new TopicSpecification
    {
        Name = "message-topic"
    };

    adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpecification }).Wait();
}
#endregion

var consumerConf = new ConsumerConfig
{
    GroupId = "consumer-group",
    BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BROKERS"),
    AutoOffsetReset = AutoOffsetReset.Earliest
};

var producerConfig = new ProducerConfig { BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BROKERS") };

using var c = new ConsumerBuilder<string, string>(consumerConf).Build();
using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
c.Subscribe("message-topic");

try
{
    while (true)
    {
        try
        {
            var cr = c.Consume();

            if (cr is null)
                continue;

            Console.WriteLine("recieved: " + cr.Message.Value);

            var kafkaMessage = new Message<string, string>
            {
                Key = cr.Message.Key,
                Value = $"Message recieved and processed: {cr.Message.Value}"
            };

            producer.Produce("response-topic", kafkaMessage);
        }
        catch (ConsumeException e)
        {
            Console.WriteLine($"Error consuming message: {e.Error.Reason}");
        }
    }
}
catch (OperationCanceledException)
{
    c.Close();
}