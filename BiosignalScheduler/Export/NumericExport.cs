using System.Collections.Generic;
using System.Linq;
using BiosignalScheduler.Model;

namespace BiosignalScheduler.Export
{
    internal class NumericExport: Scheduler.IScheduleOperator
    {
        private readonly SqlHelper _helper = SqlHelper.Instance(new SqlHelper.Connection
        {
            Server = "",
            UserId = "",
            Password = "",
            Database = ""
        });

        public void Operate(List<MqModel> data)
        {
            Filter(data).ForEach(async val => await _helper.InsertNumericValueAsync(val));
        }

        private static List<MqModel> Filter(IEnumerable<MqModel> origin) =>
            origin.Where(val => val.IsNumeric).ToList();
    }
}
