using System;
using System.Threading.Tasks;
using AmazomMqPoc.Logic.Entities;

namespace AmazomMqPoc.Logic.Data
{
    public interface IRepository
    {
        Task AddScheduledMessage(Message message);
        Task<Message> GetMessage(string id);
        Task UpdateMessage(Message message);
        Task DeleteMessage(string id);
        Task AddSubscription(Subscription subscription);
        Task<Subscription> GetSubscription(string id);
        Task UpdateSubscription(Subscription subscription);
        Task DeleteSubscription(string id);
    }
}
