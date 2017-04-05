using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using BiosignalScheduler.Model;
using static System.IO.Compression.CompressionMode;

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
                WriteToCsv(val.PatientId, val.Key, data, startDate, endDate);
                var filePath = WriteToGzipCsv(val.PatientId, val.Key, data, startDate, endDate);
                await _helper.InsertWaveformValueAsync(val, filePath, startDate, endDate);
            });
        }

        private string WriteToGzipCsv(string patientId, string key,
            List<PubsubModel> data, DateTime start, DateTime end)
        {
            var tmpPatientId = _helper.GetAnonymousId(patientId);
            // Redefine Data.
            var list = data.FindAll(obj => obj.Key.Equals(key) && obj.PatientId.Equals(patientId));
            list.Sort((x, y) => y.Timestamp.CompareTo(x.Timestamp));
            var newValues = new List<double>();
            list.ForEach(val => newValues.AddRange((List<double>)val.GetValue()));

            var startDateStr = SqlHelper.DateTimeToString(start, "{0:yyyyMMdd}");
            var endDateStr = SqlHelper.DateTimeToString(end, "{0:yyyyMMdd}");
            var startTimeStr = SqlHelper.DateTimeToString(start, "{0:HHmmss}");
            var endTimeStr = SqlHelper.DateTimeToString(end, "{0:HHmmss}");
            var folderPath = $"D:\\BiosignalRepository\\CSV\\{tmpPatientId}\\{startDateStr}\\";
            var filePath = folderPath + $"{startDateStr}{startTimeStr}_{endDateStr}{endTimeStr}_{key}.csv";

            Directory.CreateDirectory(folderPath);

            using (var file = File.Create(filePath))
            using (var gzipStream = new GZipStream(file, CompressionLevel.Fastest))
            {
                for (var i = 1; i <= newValues.Count; ++i)
                {
                    var d = newValues[i - 1];
                    var str = d.ToString(CultureInfo.InvariantCulture);
                    gzipStream.Write(Encoding.UTF8.GetBytes(str), 0, Encoding.UTF8.GetByteCount(str));

                    if (i != 1 && (i % ((newValues.Count / 60) + 1)) == 0)
                    {
                        gzipStream.Write(Encoding.UTF8.GetBytes("\n"), 0, Encoding.UTF8.GetByteCount("\n"));
                        continue;
                    }

                    gzipStream.Write(Encoding.UTF8.GetBytes(","), 0, Encoding.UTF8.GetByteCount(","));
                }
            }

            return filePath;
        }

        private string WriteToCsv(string patientId, string key,
            List<PubsubModel> data, DateTime start, DateTime end)
        {
            var tmpPatientId = _helper.GetAnonymousId(patientId);
            // Redefine Data.
            var list = data.FindAll(obj => obj.Key.Equals(key) && obj.PatientId.Equals(patientId));
            list.Sort((x, y) => y.Timestamp.CompareTo(x.Timestamp));
            var newValues = new List<double>();
            list.ForEach(val => newValues.AddRange((List<double>)val.GetValue()));

            var startDateStr = SqlHelper.DateTimeToString(start, "{0:yyyyMMdd}");
            var endDateStr = SqlHelper.DateTimeToString(end, "{0:yyyyMMdd}");
            var startTimeStr = SqlHelper.DateTimeToString(start, "{0:HHmmss}");
            var endTimeStr = SqlHelper.DateTimeToString(end, "{0:HHmmss}");
            var folderPath = $"D:\\BiosignalRepository\\CSV\\{tmpPatientId}\\{startDateStr}\\";
            var filePath = folderPath + $"{startDateStr}{startTimeStr}_{endDateStr}{endTimeStr}_{key}.csv";

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
