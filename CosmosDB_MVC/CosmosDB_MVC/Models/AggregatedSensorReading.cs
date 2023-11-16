using Newtonsoft.Json;

namespace CosmosDB_MVC.Models
{
	public class AggregatedSensorReading : SensorReadingBase
	{
		[JsonProperty(PropertyName = "MeasuringValues")]
		public double[] MeasuringValues;

		[JsonProperty(PropertyName = "TimestampSeries")]
		public DateTime[] TimestampSeries;
	}
}
