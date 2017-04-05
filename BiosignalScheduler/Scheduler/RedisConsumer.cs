using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BiosignalScheduler.Model;
using BiosignalScheduler.Util;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BiosignalScheduler.Scheduler
{
    public class RedisConsumer: AbstractConsumer<RedisConsumer>
    {

        private readonly Task<ConnectionMultiplexer> _taskMultiplexer;
        private ConnectionMultiplexer _redis;

        public RedisConsumer()
        {
            _taskMultiplexer = ConnectionMultiplexer
                .ConnectAsync("localhost:6379,localhost:6380");
        }

        public RedisConsumer(string clusterStr)
        {
            _taskMultiplexer = ConnectionMultiplexer.ConnectAsync(clusterStr);
        }
        
        public override async void Connect()
        {
            _redis = await _taskMultiplexer;
        }

        public void AddHandler(string channel, Action<RedisChannel, RedisValue> handler)
        {
            _redis.GetSubscriber().Subscribe(channel, handler);
        }

        public Action<RedisChannel, RedisValue> GetDefaultHandler()
        {
            return (channel, value) =>
            {
                var key = channel.ToString();

                if (ConsumingList[key] is null)
                {
                    ConsumingList[key] = new List<PubsubModel>();
                }
                
                var json = GzipUtil.Unzip(value.ToString());
                ConsumingList[key].Add(JsonConvert.DeserializeObject<PubsubModel>(json));
            };
        }
    }
}