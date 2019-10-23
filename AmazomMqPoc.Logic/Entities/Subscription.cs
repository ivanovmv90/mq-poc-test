using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazomMqPoc.Logic.Entities
{
    public class Subscription
    {
        public string Id { get; set; }
        public Guid ClientId { get; set; }
        public string MessageFilter { get; set; }
        public Uri WebhookUrl { get; set; }
    }
}
