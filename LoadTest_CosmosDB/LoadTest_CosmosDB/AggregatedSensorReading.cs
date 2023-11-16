using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTest_CosmosDB
{
    public class AggregatedSensorReading : SensorReadingBase
    {
        
        [JsonProperty(PropertyName = "MeasuringValues")]
        public double[] MeasuringValues;

        [JsonProperty(PropertyName = "TimestampSeries")]
        public DateTime[] TimestampSeries;
    }
}

