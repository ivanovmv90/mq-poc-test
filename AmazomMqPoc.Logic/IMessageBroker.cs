using AmazomMqPoc.Logic.Entities;
using Apache.NMS;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AmazomMqPoc.Logic
{
    public interface IMessageBroker
    {
        Task CreateMessage(Message message);
        Task UpdateMessage(Message message);
        Task<Message> GetMessage(string id);
        Task DeleteMessage(string id);
        Task TriggerMessage(string id);
        Task<IEnumerable<Message>> SearchMessages(Expression expression);
        Task CreateSubscription(Subscription subscription);
        Task<IEnumerable<Subscription>> SearchSubscription(Uri webhookUri);
        Task<Subscription> GetSubscription(string id);
        Task UpdateSubscription(Subscription subscription);
        Task DeleteSubscription(string id);
        void StartListeners(MessageListener listener1, MessageListener listener2);
    }
}
