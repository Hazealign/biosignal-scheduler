using System.Collections.Generic;
using System.Text;
using BiosignalScheduler.Model;
using BiosignalScheduler.Util;
using Castle.Core.Logging;
using Newtonsoft.Json;
using NsqSharp;

namespace BiosignalScheduler.Scheduler
{
    public sealed class NsqConsumer: AbstractConsumer<NsqConsumer>
    {
        private readonly List<Consumer> _consumers;

        public NsqConsumer()
        {
            _consumers = new List<Consumer>();
        }

        public override void Connect()
        {
            _consumers.ForEach(consumer =>
                consumer.ConnectToNsqLookupd("127.0.0.1:4161"));
        }

        public void AddHandler<T>(string channel, T handler) where T: IHandler
        {
            var consumer = new Consumer("Biosignal", channel);
            consumer.AddHandler(handler);
            _consumers.Add(consumer);
        }

        public class MessageHandler: IHandler
        {
            private readonly string _channel;
            private static readonly ConsoleLogger Logger = 
                new ConsoleLogger("MessageHandler", LoggerLevel.Debug);

            public MessageHandler(string channel)
            {
                _channel = channel;
            }

            public void HandleMessage(IMessage message)
            {
                if (Instance.ConsumingList[_channel] is null)
                {
                    Instance.ConsumingList[_channel] = new List<PubsubModel>();
                }

                var json = GzipUtil.Unzip(message.Body);
                Instance.ConsumingList[_channel]
                    .Add(JsonConvert.DeserializeObject<PubsubModel>(json));
            }

            public void LogFailedMessage(IMessage message)
            {
                Logger.Debug("Error Message");
                Logger.Error(Encoding.UTF8.GetString(message.Body));
                Logger.Error(GzipUtil.Unzip(message.Body));
            }
        }
    }
}
