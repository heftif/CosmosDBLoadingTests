using Newtonsoft.Json;


namespace LoadTest_CosmosDB
{
    public class SensorReadingBase
    {
        [JsonProperty(PropertyName = "deviceIdentifier")]
        public string DeviceIdentifier;

        [JsonProperty(PropertyName = "UnitOfMeasure")]
        public string UnitOfMeasure;

        [JsonProperty(PropertyName = "id")]
        public string Id;

        [JsonProperty(PropertyName = "ReadingInterval")]
        public string ReadingInterval;

        [JsonProperty(PropertyName = "ReadingTimestamp")]
        public DateTime ReadingTimestamp;

        [JsonProperty(PropertyName = "ReadingValue")]
        public double ReadingValue;
    }
}
