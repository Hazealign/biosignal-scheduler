using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiosignalScheduler.Model
{
    [Serializable]
    public class MqModel: ICloneable
    {
        [JsonProperty("TIMESTAMP", Required = Required.Always)]
        public DateTime Timestamp { get; set; }

        [JsonProperty("TYPE", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("PORT", Required = Required.Always)]
        public string Port { get; set; }

        [JsonProperty("HOST", Required = Required.Always)]
        public string Host { get; set; }

        [JsonProperty("BLOOD_PRESSURE_SYS")]
        public double BloodPressureSys { get; set; }

        [JsonProperty("BLOOD_PRESSURE_DIA")]
        public double BloodPressureDia { get; set; }

        [JsonProperty("BLOOD_PRESSURE_MEAN")]
        public double BloodPressureMean { get; set; }

        [JsonProperty("ET_CO2")]
        public double EtCo2 { get; set; }

        [JsonProperty("AIRWAY_RESP_RATE")]
        public double AirwayRespRate { get; set; }

        [JsonProperty("PLETH_WAVE")]
        public List<double> PlethWave { get; set; }

        [JsonProperty("HEART_RATE")]
        public double HeartRate { get; set; }

        [JsonProperty("SPO2")]
        public double SpO2 { get; set; }

        [JsonProperty("RESP_RATE")]
        public double RespRate { get; set; }

        [JsonProperty("ECG")]
        public List<double> EcgWave { get; set; }

        [JsonProperty("VALUE_UNIT", Required = Required.Always)]
        public string Unit { get; set; }

        [JsonProperty("UDID", Required = Required.Always)]
        public string UniqueDeviceId { get; set; }

        [JsonProperty("PATIENT_ID", Required = Required.Always)]
        public string PatientId { get; set; }

        public bool IsNumeric => GetValue().GetType() != typeof(List<double>);

        public object GetValue()
        {
            switch (Type)
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
            return new KeyValuePair<string, object>(Type.Contains("ECG")? "ECG": Type, GetValue());
        }

        public object Clone()
        {
            return new MqModel()
            {
                Timestamp = Timestamp,
                AirwayRespRate = AirwayRespRate,
                BloodPressureDia = BloodPressureDia,
                BloodPressureMean = BloodPressureMean,
                BloodPressureSys = BloodPressureSys,
                EcgWave = EcgWave,
                EtCo2 = EtCo2,
                HeartRate = HeartRate,
                Host = Host,
                PatientId = PatientId,
                Type = Type,
                Port = Port,
                UniqueDeviceId = UniqueDeviceId,
                Unit = Unit,
                RespRate = RespRate,
                PlethWave = PlethWave,
                SpO2 = SpO2
            };
        }
    }
}
