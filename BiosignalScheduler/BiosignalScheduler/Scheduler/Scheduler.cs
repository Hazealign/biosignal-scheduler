using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
            _loop = Observable.Interval(new TimeSpan(0, 10, 0))
                .StartWith(0L)
                .Select(value => value + 1)
                .Select(value => ConsumerManager.Instance.ConsumingList)
                .SubscribeOn(NewThreadScheduler.Default)
                .Retry()
                .Subscribe(list =>
                {
                    Operators.ForEach(op =>
                    {
                        op.Operate(list);
                    });

                    list.Clear();
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
