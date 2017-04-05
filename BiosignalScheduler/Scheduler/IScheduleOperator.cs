using System.Collections.Generic;
using BiosignalScheduler.Model;

namespace BiosignalScheduler.Scheduler
{
    public interface IScheduleOperator
    {
        void Operate(List<PubsubModel> data);
    }
}
