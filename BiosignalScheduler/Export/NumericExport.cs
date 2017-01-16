using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiosignalScheduler.Model;

namespace BiosignalScheduler.Export
{
    class NumericExport: Scheduler.IScheduleOperator
    {
        public void Operate(List<MqModel> data)
        {
            Console.WriteLine(data);
        }
    }
}
