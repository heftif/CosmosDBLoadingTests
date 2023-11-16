using Newtonsoft.Json;

namespace CosmosDB_MVC.Models
{
	public class SensorReading : SensorReadingBase
	{
		[JsonProperty(PropertyName = "ValueCategory")]
		public string ValueCategory;

		[JsonProperty(PropertyName = "ValueQuality")]
		public string ValueQuality;

		[JsonProperty(PropertyName = "AdditionalValue")]
		public string AdditionalValue;
	}
}

