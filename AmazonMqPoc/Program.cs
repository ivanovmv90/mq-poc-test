using System;
using System.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Util;

namespace AmazonMqPoc
{
    class Program
    {
        protected static AutoResetEvent semaphore = new AutoResetEvent(false);
        protected static ITextMessage message = null;
        protected static TimeSpan receiveTimeout = TimeSpan.FromSeconds(10);

        public static void Main(string[] args)
        {
            Uri connecturi = new Uri("stomp+ssl://b-c43876a9-12c0-4c75-8580-f836108b30dc-1.mq.us-east-2.amazonaws.com:61614");

            // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.
            IConnectionFactory factory = new ConnectionFactory(connecturi);

            using (IConnection connection = factory.CreateConnection("mq-test", "qwerty123456"))
            using (ISession session = connection.CreateSession())
            {
                // Examples for getting a destination:
                //
                // Hard coded destinations:
                //    IDestination destination = session.GetQueue("FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //    IDestination destination = session.GetTopic("FOO.BAR");
                //    Debug.Assert(destination is ITopic);
                //
                // Embedded destination type in the name:
                //    IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //    IDestination destination = SessionUtil.GetDestination(session, "topic://FOO.BAR");
                //    Debug.Assert(destination is ITopic);
                //
                // Defaults to queue if type is not specified:
                //    IDestination destination = SessionUtil.GetDestination(session, "FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //
                // .NET 3.5 Supports Extension methods for a simplified syntax:
                //    IDestination destination = session.GetDestination("queue://FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //    IDestination destination = session.GetDestination("topic://FOO.BAR");
                //    Debug.Assert(destination is ITopic);
                IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");

                Console.WriteLine("Using destination: " + destination);

                // Create a consumer and producer
                using (IMessageConsumer consumer = session.CreateConsumer(destination))
                using (IMessageProducer producer = session.CreateProducer(destination))
                {
                    // Start the connection so that messages will be processed.
                    connection.Start();
                    producer.DeliveryMode = MsgDeliveryMode.Persistent;
                    producer.RequestTimeout = receiveTimeout;

                    consumer.Listener += new MessageListener(OnMessage);

                    // Send a message
                    ITextMessage request = session.CreateTextMessage("Hello World!");
                    request.NMSCorrelationID = "abc";
                    request.Properties["NMSXGroupID"] = "cheese";
                    request.Properties["myHeader"] = "Cheddar";

                    producer.Send(request);

                    // Wait for the message
                    semaphore.WaitOne((int)receiveTimeout.TotalMilliseconds, true);

                    if (message == null)
                    {
                        Console.WriteLine("No message received!");
                    }
                    else
                    {
                        Console.WriteLine("Received message with ID:   " + message.NMSMessageId);
                        Console.WriteLine("Received message with text: " + message.Text);
                    }
                }
            }
        }
        
        protected static void OnMessage(IMessage receivedMsg)
        {
            message = receivedMsg as ITextMessage;
            semaphore.Set();
        }
    }
}
