using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiosignalScheduler.Model
{
    [Serializable]
    public class MqModel
    {
        [JsonProperty("TIMESTAMP", Required = Required.Always)]
        public DateTime Timestamp { get; set; }

        [JsonProperty("TYPE", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("PORT", Required = Required.Always)]
        public string Port { get; set; }

        [JsonProperty("HOST", Required = Required.Always)]
        public string Host { get; set; }

        [JsonProperty("KEY", Required = Required.Always)]
        public string Key { get; set; }

        [JsonProperty("BLOOD_PRESSURE_SYS", Required = Required.AllowNull)]
        public int BloodPressureSys { get; set; }

        [JsonProperty("BLOOD_PRESSURE_DIA", Required = Required.AllowNull)]
        public int BloodPressureDia { get; set; }

        [JsonProperty("BLOOD_PRESSURE_MEAN", Required = Required.AllowNull)]
        public int BloodPressureMean { get; set; }

        [JsonProperty("ET_CO2", Required = Required.AllowNull)]
        public int EtCo2 { get; set; }

        [JsonProperty("AIRWAY_RESP_RATE", Required = Required.AllowNull)]
        public int AirwayRespRate { get; set; }

        [JsonProperty("PLETH_WAVE", Required = Required.AllowNull)]
        public List<double> PlethWave { get; set; }

        [JsonProperty("HEART_RATE", Required = Required.AllowNull)]
        public int HeartRate { get; set; }

        [JsonProperty("SPO2", Required = Required.AllowNull)]
        public int SpO2 { get; set; }

        [JsonProperty("RESP_RATE", Required = Required.AllowNull)]
        public int RespRate { get; set; }

        [JsonProperty("ECG", Required = Required.AllowNull)]
        public List<double> EcgWave { get; set; }

        [JsonProperty("VALUE_UNIT", Required = Required.Always)]
        public string Unit { get; set; }

        [JsonProperty("UDID", Required = Required.Always)]
        public string UniqueDeviceId { get; set; }

        [JsonProperty("PATIENT_ID", Required = Required.Always)]
        public string PatientId { get; set; }

        public bool IsNumeric => GetValue().GetType() == typeof(List<double>);

        public object GetValue()
        {
            switch (Key)
            {
                case "BLOOD_PRESSURE_SYS":
                    return BloodPressureSys;
                case "BLOOD_PRESSURE_DIA":
                    return BloodPressureDia;
                case "BLOOD_PRESSURE_MEAN":
                    return BloodPressureMean;
                case "ET_CO2":
                    return EtCo2;
                case "AIRWAY_RESP_RATE":
                    return AirwayRespRate;
                case "PLETH_WAVE":
                    return PlethWave;
                case "HEART_RATE":
                    return HeartRate;
                case "SPO2":
                    return SpO2;
                case "RESP_RATE":
                    return RespRate;
                default:
                    return EcgWave;
            }
        }

        public KeyValuePair<string, object> GetKeyValue()
        {
            return new KeyValuePair<string, object>(Key.Contains("ECG")? "ECG": Key, GetValue());
        }
    }
}
