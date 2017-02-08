using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BiosignalScheduler.Model;
using Ploeh.AutoFixture;

namespace BiosignalScheduler.Export
{
    internal class WaveformExport: Scheduler.IScheduleOperator
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
            var startDate = DateTime.Now;
            var endDate = new DateTime(startDate.Ticks + 1000 * 60 * 10);

            Filter(data).ForEach(async val =>
            {
                var filePath = WriteToCsv(val.PatientId, val.Type, data, startDate, endDate);
                await _helper.InsertWaveformValueAsync(val, filePath, startDate, endDate);
            });
        }

        private string WriteToCsv(string patientId, string type, 
            List<MqModel> data, DateTime startDate, DateTime endDate)
        {
            var tmpPatientId = _helper.GetAnonymousId(patientId);
            // Redefine Data.
            var list = data.FindAll(obj => obj.Type.Equals(type) && obj.PatientId.Equals(patientId));
            list.Sort((x, y) => y.Timestamp.CompareTo(x.Timestamp));
            var newValues = new List<double>();
            list.ForEach(val => newValues.AddRange((List<double>) val.GetValue()));
            
            var startDateStr = SqlHelper.DateTimeToString(startDate, "{0:yyyyMMdd}");
            var endDateStr = SqlHelper.DateTimeToString(endDate, "{0:yyyyMMdd}");
            var startTimeStr = SqlHelper.DateTimeToString(startDate, "{0:HHmmss}");
            var endTimeStr = SqlHelper.DateTimeToString(endDate, "{0:HHmmss}");
            var folderPath = $"D:\\BiosignalRepository\\CSV\\{tmpPatientId}\\{startDateStr}\\";
            var filePath = folderPath + $"{startDateStr}{startTimeStr}_{endDateStr}{endTimeStr}_{type}.csv";

            Directory.CreateDirectory(folderPath);
            using (var file = File.CreateText(filePath))
            {
                for (var i = 1; i <= newValues.Count; ++i)
                {
                    var d = newValues[i - 1];
                    file.Write(d.ToString(CultureInfo.InvariantCulture));

                    if (i != 1 && (i % ((newValues.Count / 60) + 1)) == 0)
                    {
                        file.Write("\n");
                        continue;
                    }

                    file.Write(",");
                }
            }

            return filePath;
        }

        private static List<MqModel> Filter(IEnumerable<MqModel> origin) =>
            origin.Where(val => !val.IsNumeric).Distinct(new TypeComparer()).ToList();

        internal class TypeComparer : EqualityComparer<MqModel>
        {
            public override bool Equals(MqModel x, MqModel y)
            {
                if (x == null || y == null) return false;
                return x.PatientId.Equals(y.PatientId) && x.Type.Equals(y.Type);
            }

            public override int GetHashCode(MqModel obj)
            {
                if (obj == null) return 0;
                return obj.PatientId.GetHashCode() ^ obj.Type.GetHashCode();
            }
        }
    }
}
