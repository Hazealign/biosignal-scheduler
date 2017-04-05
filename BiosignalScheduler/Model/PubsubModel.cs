using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BiosignalScheduler.Model
{
    [Serializable]
    public class PubsubModel: ICloneable
    { 
        public enum DataType
        {
            Numeric, Waveform
        }

        public enum DeviceType { Philips, Hamilton }

        [JsonProperty("TIMESTAMP", Required = Required.Always)]
        public DateTime Timestamp { get; set; }

        [JsonProperty("KEY", Required = Required.Always)]
        public string Key { get; set; }

        [JsonProperty("PORT", Required = Required.Always)]
        public string Port { get; set; }

        [JsonProperty("HOST", Required = Required.Always)]
        public string Host { get; set; }
       
        [JsonProperty("VALUE_UNIT", Required = Required.Always)]
        public string Unit { get; set; }

        [JsonProperty("UDID", Required = Required.Always)]
        public string UniqueDeviceId { get; set; }

        [JsonProperty("PATIENT_ID", Required = Required.Always)]
        public string PatientId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("TYPE", Required = Required.Always)]
        public DataType Type { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("DEVICE", Required = Required.Always)]
        public DeviceType Device { get; set; }

        [JsonProperty("NUMERIC_VALUE")]
        public double NumericValue { get; set; }

        [JsonProperty("WAVEFORM_VALUE")]
        public List<double> WaveformValue { get; set; }

        public bool IsNumeric => Type == DataType.Numeric;

        public object GetValue()
        {
            if (IsNumeric) return NumericValue;
            return WaveformValue;
        }

        public KeyValuePair<string, object> GetKeyValue()
        {
            return new KeyValuePair<string, object>(Key, GetValue());
        }

        public object Clone()
        {
            return new PubsubModel()
            {
                Timestamp = Timestamp,
                Host = Host,
                PatientId = PatientId,
                Key = Key,
                Type = Type,
                Port = Port,
                Device = Device,
                UniqueDeviceId = UniqueDeviceId,
                Unit = Unit,
                NumericValue = NumericValue,
                WaveformValue = WaveformValue
            };
        }
    }
}
