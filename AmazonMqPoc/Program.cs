using System;
using System.Collections.Generic;
using System.Threading;
using AmazomMqPoc.Logic;
using AmazomMqPoc.Logic.Entities;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Util;
using Microsoft.Extensions.DependencyInjection;

namespace AmazonMqPoc
{
    class Program
    {
        private static IServiceProvider _serviceProvider;

        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddServices();
            _serviceProvider = services.BuildServiceProvider();
            using(var scope = _serviceProvider.CreateScope())
            {
                var msgBroker = _serviceProvider.GetService<IMessageBroker>();
                msgBroker.StartListeners(OnMsg1, OnMsg2);

                var msgId1 = Guid.NewGuid().ToString().Replace("-", "");
                var msgId2 = Guid.NewGuid().ToString().Replace("-", "");
                var message1 = new Message 
                { 
                    Body = "Foo",
                    CustomHeaders = new Dictionary<string, string> { ["FLAG"] = "1"},
                    Id = msgId1,
                    Schedule = new Schedule { ScheduleCronString = "42 * * * *", Status = ScheduleStatus.Active }
                };
                var message2 = new Message
                {
                    Body = "Bar",
                    CustomHeaders = new Dictionary<string, string> { ["FLAG"] = "2" },
                    Id = msgId2,
                    Schedule = new Schedule { ScheduleCronString = "44 * * * *", Status = ScheduleStatus.Active }
                };
                msgBroker.CreateMessage(message1).Wait();
                msgBroker.CreateMessage(message2).Wait();
                msgBroker.TriggerMessage(msgId1).Wait();
                msgBroker.TriggerMessage(msgId2).Wait();
                msgBroker.DeleteMessage(msgId1).Wait();
                msgBroker.DeleteMessage(msgId2).Wait();
                Console.ReadKey();
            }
        }

        private static void OnMsg1(IMessage message)
        {
            var msg = message as ITextMessage;
            var flag = msg.Properties["FLAG"].ToString();
            Console.WriteLine($"From simple consumer: {msg.Text}, flag = {flag}");
        }

        private static void OnMsg2(IMessage message)
        {
            var msg = message as ITextMessage;
            var flag = msg.Properties["FLAG"].ToString();
            Console.WriteLine($"From durable consumer: {msg.Text}, flag = {flag}");
        }
    }
}
