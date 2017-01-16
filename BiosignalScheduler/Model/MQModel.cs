using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BiosignalScheduler.Model
{
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

        public KeyValuePair<string, object> GetKeyValue()
        {
            switch (this.Key)
            {
                case "BLOOD_PRESSURE_SYS":
                    return new KeyValuePair<string, object>(this.Key, BloodPressureSys);
                case "BLOOD_PRESSURE_DIA":
                    return new KeyValuePair<string, object>(this.Key, BloodPressureDia);
                case "BLOOD_PRESSURE_MEAN":
                    return new KeyValuePair<string, object>(this.Key, BloodPressureMean);
                case "ET_CO2":
                    return new KeyValuePair<string, object>(this.Key, EtCo2);
                case "AIRWAY_RESP_RATE":
                    return new KeyValuePair<string, object>(this.Key, AirwayRespRate);
                case "PLETH_WAVE":
                    return new KeyValuePair<string, object>(this.Key, PlethWave);
                case "HEART_RATE":
                    return new KeyValuePair<string, object>(this.Key, HeartRate);
                case "SPO2":
                    return new KeyValuePair<string, object>(this.Key, SpO2);
                case "RESP_RATE":
                    return new KeyValuePair<string, object>(this.Key, RespRate);
                default:
                    return new KeyValuePair<string, object>("ECG", EcgWave);
            }
        }
    }
}
