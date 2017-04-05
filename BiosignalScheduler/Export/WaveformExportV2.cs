using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BiosignalScheduler.Model;

namespace BiosignalScheduler.Export
{
    internal class WaveformExportV2: Scheduler.IScheduleOperator
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
            var startDate = DateTime.Now;
            var endDate = new DateTime(startDate.Ticks + 1000 * 60 * 10);

            Filter(data).ForEach(async val =>
            {
                var filePath = WriteToCsv(val.PatientId, val.Key, data, startDate, endDate);
                await _helper.InsertWaveformValueAsync(val, filePath, startDate, endDate);
            });
        }

        private string WriteToCsv(string patientId, string key,
            List<PubsubModel> data, DateTime start, DateTime end)
        {
            var tmpPatientId = _helper.GetAnonymousId(patientId);

            var startDateStr = SqlHelper.DateTimeToString(start, "{0:yyyyMMdd}");
            var endDateStr = SqlHelper.DateTimeToString(end, "{0:yyyyMMdd}");
            var startTimeStr = SqlHelper.DateTimeToString(start, "{0:HHmmss}");
            var endTimeStr = SqlHelper.DateTimeToString(end, "{0:HHmmss}");

            var folderPath = $"D:\\BiosignalRepository\\CSV\\{tmpPatientId}\\{startDateStr}\\";
            var filePath = folderPath + $"{startDateStr}{startTimeStr}_{endDateStr}{endTimeStr}_{key}.csv";

            Directory.CreateDirectory(folderPath);
            using (var file = File.CreateText(filePath))
            {
                var tmpDate = start;

                while (FilterTimestamp(tmpDate, start, end))
                {
                    var newValues = new List<double>();
                    var list = Filter(data, tmpDate, tmpDate.AddSeconds(10));
                    list.ForEach(val => newValues.AddRange((List<double>) val.GetValue()));

                    for (var i = 0; i <= newValues.Count - 1; i++)
                    {
                        file.Write(newValues[i].ToString(CultureInfo.InvariantCulture));
                        if (i != newValues.Count - 1) file.Write(",");
                    }

                    file.Write("\n");
                    tmpDate = tmpDate.AddSeconds(10);
                }
            }

            return filePath;
        }

        private static List<PubsubModel> Filter(IEnumerable<PubsubModel> origin)
            => origin.Where(val => !val.IsNumeric).Distinct(new TypeComparer()).ToList();

        private static List<PubsubModel> Filter(IEnumerable<PubsubModel> origin, DateTime start, DateTime end)
        {
            var retVal = Filter(origin).FindAll(val => FilterTimestamp(val.Timestamp, start, end));
            retVal.Sort((x, y) => y.Timestamp.CompareTo(x.Timestamp));
            return retVal;
        }
        
        private static bool FilterTimestamp(DateTime origin, DateTime start, DateTime end) 
            => DateTime.Compare(origin, end) < 0 && DateTime.Compare(origin, start) >= 0;

        internal class TypeComparer : EqualityComparer<PubsubModel>
        {
            public override bool Equals(PubsubModel x, PubsubModel y)
            {
                if (x == null || y == null) return false;
                return x.PatientId.Equals(y.PatientId) && x.Key.Equals(y.Key);
            }

            public override int GetHashCode(PubsubModel obj)
            {
                if (obj == null) return 0;
                return obj.PatientId.GetHashCode() ^ obj.Key.GetHashCode();
            }
        }
    }
}
