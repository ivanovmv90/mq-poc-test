using AmazomMqPoc.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazomMqPoc.Logic.ActiveMq
{
    public interface IActiveMqClientWrapper
    {
        Task CreateScheduledMessage(Message message);
        Task SendMessage(Message message);
    }
}
