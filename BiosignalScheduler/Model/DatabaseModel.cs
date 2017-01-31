using System;

namespace BiosignalScheduler.Model
{
    internal class DatabaseModel
    {
        [Serializable]
        public class MappingTable
        {
            public string Observation { get; set; }
            public string WaveName { get; set; }
            public long ObservationType { get; set; }
        }

        [Serializable]
        public class PatientIdMap
        {
            public string PatientId { get; set; }
            public string AnonymousId { get; set; }
        }
    }
}
