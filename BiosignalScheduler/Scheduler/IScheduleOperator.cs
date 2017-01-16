using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiosignalScheduler.Scheduler
{
    public interface IScheduleOperator
    {
        void Operate(List<dynamic> data);
    }
}
