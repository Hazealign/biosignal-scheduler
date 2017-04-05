using System.Collections.Generic;
using BiosignalScheduler.Model;

namespace BiosignalScheduler.Scheduler
{
    public abstract class AbstractConsumer<T> where T: AbstractConsumer<T>, new()
    {
        public Dictionary<string, List<PubsubModel>> ConsumingList { get; }

        protected AbstractConsumer ()
        {
            ConsumingList = new Dictionary<string, List<PubsubModel>>();
        }

        public static T Instance { get; } = new T();

        public abstract void Connect();
    }
}