using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using Castle.Core.Logging;
using Newtonsoft.Json;
using NsqSharp;

namespace BiosignalScheduler.Scheduler
{
    public sealed class ConsumerManager
    {
        public static ConsumerManager Instance { get; } = new ConsumerManager();
        public List<dynamic> ConsumingList { get; }
        private readonly Consumer _consumer;

        private ConsumerManager()
        {
            ConsumingList = new List<dynamic>();
            _consumer = new Consumer("Biosignal", "Channel");
            _consumer.AddHandler(new MessageHandler());
        }

        public void Connect()
        {
            _consumer.ConnectToNsqLookupd("127.0.0.1:4161");
        }

        public void AddHandler<T>(T handler) where T: IHandler
        {
            _consumer.AddHandler(handler);
        }

        public class MessageHandler : IHandler
        {
            private static readonly ConsoleLogger Logger = 
                new ConsoleLogger("MessageHandler", LoggerLevel.Debug);

            public void HandleMessage(IMessage message)
            {
                var json = Encoding.UTF8.GetString(message.Body);
                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(json);
                Instance.ConsumingList.Add(obj);
            }

            public void LogFailedMessage(IMessage message)
            {
                Logger.Debug("Error Message");
                Logger.Error(Encoding.UTF8.GetString(message.Body));
            }
        }
    }
}
