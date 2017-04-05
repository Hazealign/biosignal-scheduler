using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using BiosignalScheduler.Model;
using Castle.Core.Logging;

namespace BiosignalScheduler.Scheduler
{
    public class Scheduler
    {
        private IDisposable _loop;
        private readonly ConsoleLogger _logger;
        private List<IScheduleOperator> Operators { get; }

        public Scheduler()
        {
            const string channelStr = "Channel";
            _logger = new ConsoleLogger("Scheduler", LoggerLevel.Error);
            Operators = new List<IScheduleOperator>();
            NsqConsumer.Instance.AddHandler(channelStr, new NsqConsumer.MessageHandler(channelStr));
            NsqConsumer.Instance.Connect();
        }

        public void Start()
        {
            var offset = new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 
                DateTime.Now.Hour, IntegerRound(DateTime.Now.Minute) , 0, new TimeSpan(0)) - DateTimeOffset.Now;

            _loop = Observable.Timer(offset)
                .Select(value => Observable.Interval(new TimeSpan(0, 10, 0)).StartWith(0L))
                .Select(value =>
                {
                    var consumingList = NsqConsumer.Instance.ConsumingList;
                    var list = new List<PubsubModel>();

                    // Disable Lint. Because IT NEEDS HARD FOR-LOOP!
                    // ReSharper disable once ForCanBeConvertedToForeach
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    for (var i = 0; i < consumingList.Values.Count; i++)
                    {
                        for (var j = 0; i < consumingList.Values.ElementAt(i).Count; j++)
                        {
                            list.Add(consumingList.Values.ElementAt(i)[j].Clone() as PubsubModel);
                        }

                        consumingList.Values.ElementAt(i).Clear();
                    }

                    return list;
                })
                .SubscribeOn(NewThreadScheduler.Default)
                .Retry()
                .Subscribe(list =>
                {
                    foreach (var op in Operators) op.Operate(list);
                }, err =>
                {
                    _logger.Error(err.StackTrace);
                });
        }

        public void Stop()
        {
            _loop?.Dispose();
        }

        public void AddOperator<T>(T oper) where T : IScheduleOperator
        {
            Operators.Add(oper);
        }

        public static int IntegerRound(int i) => ((int) Math.Round(i / 10.0)) * 10;
    }
}
