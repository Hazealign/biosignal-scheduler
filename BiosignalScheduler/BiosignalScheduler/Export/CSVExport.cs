using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiosignalScheduler.Export
{
    class CSVExport: Scheduler.IScheduleOperator
    {
        public void Operate(List<dynamic> data)
        {
            Console.WriteLine(data);
        }
    }
}
