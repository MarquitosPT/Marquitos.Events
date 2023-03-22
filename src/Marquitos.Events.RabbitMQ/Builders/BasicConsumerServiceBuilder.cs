using Marquitos.Events.RabbitMQ.Consumers;
using Marquitos.Events.RabbitMQ.Services;

namespace Marquitos.Events.RabbitMQ.Builders
{
    /// <summary>
    /// Basic Consumer Service Builder
    /// </summary>
    /// <typeparam name="TConsumer"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    public class BasicConsumerServiceBuilder<TConsumer, TMessage> where TConsumer : BasicConsumer<TMessage> where TMessage : class
    {
        
        private BasicConsumerService<TConsumer, TMessage> _consumerService;

        internal BasicConsumerServiceBuilder(BasicConsumerService<TConsumer, TMessage> consumerService)
        {
            _consumerService = consumerService;
        }

        /// <summary>
        /// Subscribes the specified queue name
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public BasicConsumerServiceBuilder<TConsumer, TMessage> WithQueueName(string queueName)
        {
            _consumerService.SetQueueName(queueName);

            return this;
        }

        /// <summary>
        /// Subscribes the specified topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public BasicConsumerServiceBuilder<TConsumer, TMessage> WithTopic(string topic)
        {
            _consumerService.AddTopic(topic);

            return this;
        }

        /// <summary>
        /// Subscribes the specified topics
        /// </summary>
        /// <param name="topics"></param>
        /// <returns></returns>
        public BasicConsumerServiceBuilder<TConsumer, TMessage> WithTopics(string[] topics)
        {
            _consumerService.AddTopics(topics);

            return this;
        }
    }
}
