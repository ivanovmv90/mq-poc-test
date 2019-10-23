using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AmazomMqPoc.Logic.Data;
using AmazomMqPoc.Logic.Entities;
using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace AmazomMqPoc.Logic
{
    public class MessageBroker : IMessageBroker, IDisposable
    {
        private const string HEADER_MESSAGE_ID = "MessageId";

        private readonly IRepository _repository;
        private readonly IConnection _connection;
        private readonly ISession _session;
        private IMessageConsumer _messageConsumer1;
        private IMessageConsumer _messageConsumer2;

        public MessageBroker(IRepository repository)
        {
            _repository = repository;
            var connectUri = new Uri(Config.ACTIVE_MQ_ENDPOINT);
            var factory = new ConnectionFactory(connectUri);
            _connection = factory.CreateConnection(Config.USERNAME, Config.PASSWORD);
            _connection.ClientId = Config.CLIENT_ID;
            _session = _connection.CreateSession();
            _connection.Start();
        }

        public async Task CreateMessage(Message message)
        {
            SendMessage(message);
            message.JobId = GetScheduledJobId(message.Id);
            await _repository.AddScheduledMessage(message).ConfigureAwait(false);
        }

        public async Task CreateSubscription(Subscription subscription)
        {
            await _repository.AddSubscription(subscription).ConfigureAwait(false);
        }

        public async Task DeleteMessage(string id)
        {
            var message = await _repository.GetMessage(id).ConfigureAwait(false);
            RemoveScheduledMessage(message.JobId);
            await _repository.DeleteMessage(id);

        }

        public async Task DeleteSubscription(string id)
        {
            _session.DeleteDurableConsumer(id.ToString());
            await _repository.DeleteSubscription(id).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _connection?.Close();
            _session?.Dispose();
            _connection?.Dispose();
            _messageConsumer1?.Dispose();
            _messageConsumer2?.Dispose();
        }

        public Task<Message> GetMessage(string id)
        {
            return _repository.GetMessage(id);
        }

        public Task<Subscription> GetSubscription(string id)
        {
            return _repository.GetSubscription(id);
        }

        public Task<IEnumerable<Message>> SearchMessages(Expression expression)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Subscription>> SearchSubscription(Uri webhookUri)
        {
            throw new NotImplementedException();
        }

        public async Task TriggerMessage(string id)
        {
            var message = await _repository.GetMessage(id).ConfigureAwait(false);
            SendMessage(message, true);
        }

        public async Task UpdateMessage(Message message)
        {
            RemoveScheduledMessage(message.JobId);
            await _repository.UpdateMessage(message).ConfigureAwait(false);
            SendMessage(message);
        }

        public async Task UpdateSubscription(Subscription subscription)
        {
            await _repository.UpdateSubscription(subscription).ConfigureAwait(false);
        }

        //TODO: replace isTriggered logic
        private void SendMessage(Message message, bool isTriggered = false)
        {
            var topic = _session.GetTopic(Config.TEST_TOPIC_NAME);

            using (var producer = _session.CreateProducer(topic))
            {
                var request = _session.CreateTextMessage(message.Body);
                request.Properties[HEADER_MESSAGE_ID] = message.Id;
                if (!isTriggered)
                {
                    request.Properties[ScheduledMessage.AMQ_SCHEDULED_CRON] = message.Schedule.ScheduleCronString;
                }
                foreach (var header in message.CustomHeaders)
                {
                    request.Properties[header.Key] = header.Value;
                }
                producer.Send(request);
            }
        }

        private string GetScheduledJobId(string messageId)
        {
            var browseTopic = _session.GetTopic(ScheduledMessage.AMQ_SCHEDULER_MANAGEMENT_DESTINATION);
            var replyQueue = _session.GetQueue(Config.SCHEDULER_MANAGEMENT_REPLY_QUEUE);

            using(var producer = _session.CreateProducer(browseTopic))
            using(var consumer = _session.CreateConsumer(replyQueue))
            using (var semaphore = new AutoResetEvent(false))
            {
                string jobId = null;
                consumer.Listener += msg => {
                    try
                    {
                        var msgId = msg.Properties[HEADER_MESSAGE_ID].ToString();
                        if (msgId == messageId)
                        {
                            jobId = msg.Properties[ScheduledMessage.AMQ_SCHEDULED_ID].ToString();
                            semaphore.Set();
                        }
                    }
                    catch(Exception)
                    {
                        //Do something?
                    }
                };

                var browseRequest = _session.CreateMessage();
                browseRequest.Properties[ScheduledMessage.AMQ_SCHEDULER_ACTION] = ScheduledMessage.AMQ_SCHEDULER_ACTION_BROWSE;
                browseRequest.NMSReplyTo = replyQueue;
                producer.Send(browseRequest);
                semaphore.WaitOne(TimeSpan.FromSeconds(10));

                //should throw if null
                return jobId;
            }
        }

        private void RemoveScheduledMessage(string jobId)
        {
            var topic = _session.GetTopic(ScheduledMessage.AMQ_SCHEDULER_MANAGEMENT_DESTINATION);
            using (var producer = _session.CreateProducer(topic))
            {
                var removeRequest = _session.CreateMessage();
                removeRequest.Properties[ScheduledMessage.AMQ_SCHEDULER_ACTION] = ScheduledMessage.AMQ_SCHEDULER_ACTION_REMOVE;
                removeRequest.Properties[ScheduledMessage.AMQ_SCHEDULED_ID] = jobId;
                producer.Send(removeRequest);
            }
        }

        public void StartListeners(MessageListener listener1, MessageListener listener2)
        {
            var topic = _session.GetTopic(Config.TEST_TOPIC_NAME);
            _messageConsumer1 = _session.CreateConsumer(topic);
            _messageConsumer2 = _session.CreateDurableConsumer(topic, "testConsumer", "FLAG = '1'", false);
            _messageConsumer1.Listener += listener1;
            _messageConsumer2.Listener += listener2;
        }

    }
}
