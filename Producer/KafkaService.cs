using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Producer
{
    public class KafkaService
    {
        public KafkaService()
        {
            var producerConfig = new ProducerConfig() { BootstrapServers = _bootstrapServers };
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
            CreateTopicIfNotExists("response-topic");
        }

        private static readonly string _bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BROKERS");

        private IProducer<string, string> _producer;

        public void SendMessage(string correlationId, string message)
        {
            var kafkaMessage = new Message<string, string>
            {
                Key = correlationId,
                Value = message
            };

            _producer.Produce("message-topic", kafkaMessage);
        }

        public string ConsumeMessage(string correlationId)
        {
            try
            {
                var consumerConfig = new ConsumerConfig()
                {
                    GroupId = "producer-group" + correlationId,
                    BootstrapServers = _bootstrapServers,
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };

                using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
                consumer.Subscribe("response-topic");

                while (true)
                {
                    var consumeResult = consumer.Consume();
                    if (consumeResult != null && consumeResult.Message.Key.Equals(correlationId))
                    {
                        Console.WriteLine("processed: " + consumeResult.Message.Value);
                        return consumeResult.Message.Value;
                    }
                    else if (consumeResult != null)
                    {
                        Console.WriteLine("not processed: " + consumeResult.Message.Value);
                    }
                }
            }
            catch (ConsumeException e)
            {
                return $"Consume error: {e.Error.Reason}";
            }
        }

        private void CreateTopicIfNotExists(string topicName)
        {
            var config = new AdminClientConfig { BootstrapServers = _bootstrapServers };

            using var adminClient = new AdminClientBuilder(config).Build();
            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                if (!metadata.Topics.Any(t => t.Topic == topicName))
                {
                    var topicSpecification = new TopicSpecification
                    {
                        Name = topicName,
                        NumPartitions = 1,
                        ReplicationFactor = 1
                    };

                    adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpecification }).Wait();
                    Console.WriteLine($"Topic {topicName} created successfully.");
                }
            }
            catch (CreateTopicsException e)
            {
                Console.WriteLine($"An error occurred creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
            }
        }
    }
}