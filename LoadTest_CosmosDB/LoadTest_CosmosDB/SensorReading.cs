using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTest_CosmosDB
{
    public class SensorReading : SensorReadingBase
    {

        [JsonProperty(PropertyName = "ValueCategory")]
        public string ValueCategory;

        [JsonProperty(PropertyName = "ValueQuality")]
        public string ValueQuality;

        [JsonProperty(PropertyName = "AdditionalValue")]
        public string AdditionalValue;


        public static List<SensorReading> GenerateRandomDailySensorData()
        {
            //pre define some constraints and seed
            Random r = new Random();
            List<SensorReading> sensorReadings = new List<SensorReading>();

            var deviceId = Guid.NewGuid().ToString();
            double readingValue = 0;

            int numberOfValues = r.Next(150, 300);

            DateTime timestamp = DateTime.Parse("2022-02-01");

            var units = new[] { "kWh", "m3" };
            var categories = new[] { "endofday", "endofmonth", "unknown" };
            var quality = new[] { "good", "unknown" };

            var unit = units[r.Next(0, 1)];
            var category = categories[0];

            if (unit == "kWh")
            {
                readingValue = r.Next(30000, 40000);
            }
            else
            {
                readingValue = r.NextDouble() * 200;
            }

            //generate data points
            for (int i = 0; i < numberOfValues; i++)
            {
                SensorReading sensorReading = new SensorReading
                {
                    DeviceIdentifier = deviceId,
                    ReadingTimestamp = timestamp,
                    ReadingValue = readingValue,
                    UnitOfMeasure = unit,
                    ValueCategory = category,
                    ValueQuality = quality[r.Next(0, 1)],
                    ReadingInterval = "daily",
                    Id = Guid.NewGuid().ToString()
                };

                sensorReadings.Add(sensorReading);

                if (unit == "kWh")
                {
                    readingValue = readingValue + r.Next(100, 200);
                }
                else
                {
                    readingValue = readingValue + r.NextDouble() * 1.5;
                }

                timestamp = timestamp.AddDays(1);
            }

            return sensorReadings;
        }



        public static List<SensorReading> GenerateRandomQuarterHourlySensorData()
        {
            //pre define some constraints and seed
            Random r = new Random();
            List<SensorReading> sensorReadings = new List<SensorReading>();

            var deviceId = Guid.NewGuid().ToString();
            double readingValue = 0;

            int numberOfValues = r.Next(150, 300) * 100;

            DateTime timestamp = DateTime.Parse("2022-02-01");

            var units = new[] { "kWh", "m3" };
            var categories = new[] { "endofday", "endofmonth", "frequent" };
            var quality = new[] { "good", "unknown" };

            var unit = units[r.Next(0, 1)];
            var category = categories[2];

            if (unit == "kWh")
            {
                readingValue = r.Next(30000, 40000);
            }
            else
            {
                readingValue = r.NextDouble() * 200;
            }

            //generate data points
            for (int i = 0; i < numberOfValues; i++)
            {
                SensorReading sensorReading = new SensorReading
                {
                    DeviceIdentifier = deviceId,
                    ReadingTimestamp = timestamp,
                    ReadingValue = readingValue,
                    UnitOfMeasure = unit,
                    ValueCategory = category,
                    ValueQuality = quality[r.Next(0, 1)],
                    ReadingInterval = "quarterHourly",
                    Id = Guid.NewGuid().ToString()
                };

                sensorReadings.Add(sensorReading);

                if (unit == "kWh")
                {
                    readingValue = readingValue + r.Next(1, 10);
                }
                else
                {
                    readingValue = readingValue + r.NextDouble();
                }

                timestamp = timestamp.AddMinutes(15);
            }

            return sensorReadings;
        }
    }
}
