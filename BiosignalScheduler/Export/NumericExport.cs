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

        public void Operate(List<PubsubModel> data)
        {
            Filter(data).ForEach(async val => await _helper.InsertNumericValueAsync(val));
        }

        private static List<PubsubModel> Filter(IEnumerable<PubsubModel> origin) =>
            origin.Where(val => val.IsNumeric).ToList();
    }
}
