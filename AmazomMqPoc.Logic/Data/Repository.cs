using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmazomMqPoc.Logic.Entities;

namespace AmazomMqPoc.Logic.Data
{
    public class Repository : IRepository
    {
        private readonly List<Message> _messages = new List<Message>();
        private readonly List<Subscription> _subscriptions = new List<Subscription>();

        public Task AddScheduledMessage(Message message)
        {
            _messages.Add(message);
            return Task.CompletedTask;
        }

        public Task AddSubscription(Subscription subscription)
        {
            _subscriptions.Add(subscription);
            return Task.CompletedTask;
        }

        public async Task DeleteMessage(string id)
        {
            var msg = await GetMessage(id).ConfigureAwait(false);
            if(msg != null)
            {
                _messages.Remove(msg);
            }
        }

        public async Task DeleteSubscription(string id)
        {
            var sub = await GetSubscription(id).ConfigureAwait(false);
            if (sub != null)
            {
                _subscriptions.Remove(sub);
            }
        }

        public Task<Message> GetMessage(string id)
        {
            return Task.FromResult(_messages.FirstOrDefault(x => x.Id == id));
        }

        public Task<Subscription> GetSubscription(string id)
        {
            return Task.FromResult(_subscriptions.FirstOrDefault(x => x.Id == id));
        }

        public async Task UpdateMessage(Message message)
        {
            var oldMsg = await GetMessage(message.Id).ConfigureAwait(false);
            _messages.Remove(oldMsg);
            _messages.Add(message);
        }

        public async Task UpdateSubscription(Subscription subscription)
        {
            var oldSub = await GetSubscription(subscription.Id).ConfigureAwait(false);
            _subscriptions.Remove(oldSub);
            _subscriptions.Add(subscription);
        }
    }
}
