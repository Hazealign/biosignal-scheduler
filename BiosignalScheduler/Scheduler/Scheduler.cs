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
            _logger = new ConsoleLogger("Scheduler", LoggerLevel.Error);
            Operators = new List<IScheduleOperator>();
            ConsumerManager.Instance.Connect();
        }

        public void Start()
        {
            _loop = Observable.Interval(new TimeSpan(0, 1, 0))
                .StartWith(0L)
                .Select(value => value + 1)
                .Select(value =>
                {
                    var list = new List<MqModel>();
                    for (var i = 0; i < ConsumerManager.Instance.ConsumingList.Count; i++)
                        list.Add(ConsumerManager.Instance.ConsumingList[i].Clone() as MqModel);
                    ConsumerManager.Instance.ConsumingList.Clear();
                    return list;
                })
                .SubscribeOn(NewThreadScheduler.Default)
                .Retry()
                .Subscribe(list =>
                { 
                    ConsumerManager.Instance.ConsumingList.Clear();
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
    }
}
