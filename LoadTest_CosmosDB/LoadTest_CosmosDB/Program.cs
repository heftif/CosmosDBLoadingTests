using Azure;
using Azure.Core;
using Bogus;
using Bogus.DataSets;
using LoadTest_CosmosDB;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Dynamic;
using System.Text;
using Database = Microsoft.Azure.Cosmos.Database;

public class Program
{
    //directory for storing and retrieving sensor data locally for upload to CosmosDB
    static string filePath = "D:\\SensorOutput\\Test6";

    public static async Task Main(string[] args)
    {
        //define environment variable
        var root = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(root, ".env");
        DotEnv.Load(dotenv);

        using CosmosClient client = new(
            accountEndpoint: Environment.GetEnvironmentVariable("ACCOUNT"),
            authKeyOrResourceToken: Environment.GetEnvironmentVariable("KEY") +"=="!
        );

        Database database = client.GetDatabase(Environment.GetEnvironmentVariable("DATABASE_NAME"));
        Container container = database.GetContainer(Environment.GetEnvironmentVariable("CONTAINER_NAME"));

        //uncomment what ever action you like to do

        //generate sensor simulation data
        //await GenerateSimulationData();

        //aggregate the quarter hourly data in an aggregate file to save space and calls to the db
        //await AggregateQuarterHourlyData();

        //insert a single simulation data value
        //await InsertSingleItem(container);

        //bulk import multiple simulation datas
        //await BulkImportDataLoading(container);

        //query the data from the db in form of a time series
        //await QueryTimeSeriesData(container);

        //query the aggregated data values 
        //await QueryAggTimeSeriesData(container);

    }


    private static async Task GenerateSimulationData()
    {
        int maxDaily = 1000;
        int maxQuarterHourly = 10;

        //generat daily data
        for (int i = 0; i < maxDaily; i++)
        {
            Console.WriteLine($"Simulating Device {i} of {maxDaily}");

            //generate random data with help of bogus package
            var sensorReadings = SensorReading.GenerateRandomDailySensorData();

            //save the data in the filepath specified
            var dailyReadingFilePath = Path.Combine(filePath, "DailyReadings");
            var sensorDirectory = Directory.CreateDirectory(Path.Combine(dailyReadingFilePath, sensorReadings[0].DeviceIdentifier)).FullName;

            //save the readings
            foreach (var reading in sensorReadings)
            {
                string timestampFormat = reading.ReadingTimestamp.ToString("yyyy-MM-dd_HH-mm");
                string fileName = timestampFormat + ".json";

                JSONFileUtils.PrettyWrite(reading, Path.Combine(sensorDirectory, fileName));
            }
        }

        //generate quarterhourly data
        for (int i = 0; i < maxQuarterHourly; i++)
        {
            Console.WriteLine($"Simulating Device {i} of {maxQuarterHourly}");

            //generate random data with help of bogus package
            var sensorReadings = SensorReading.GenerateRandomQuarterHourlySensorData();

            //save the data in the filepath specified
            var quarterlyReadingFilePath = Path.Combine(filePath, "QuarterHourlyReadings");
            var sensorDirectory = Directory.CreateDirectory(Path.Combine(quarterlyReadingFilePath, sensorReadings[0].DeviceIdentifier)).FullName;

            //save the readings
            foreach (var reading in sensorReadings)
            {
                string timestampFormat = reading.ReadingTimestamp.ToString("yyyy-MM-dd_HH-mm");
                string fileName = timestampFormat + ".json";

                JSONFileUtils.PrettyWrite(reading, Path.Combine(sensorDirectory, fileName));
            }
        }

    }

    //aggregates the quarter hourly data into a single data entry per day
    private static async Task AggregateQuarterHourlyData()
    {
        Console.WriteLine($"Aggregating Quarter Hourly Data into Daily Data...");

        //get sensor files
        var quarterlyReadingFilePath = Path.Combine(filePath, "QuarterHourlyReadings");
        var sensorFolder = Directory.GetDirectories(quarterlyReadingFilePath);

        foreach (var sensor in sensorFolder)
        {
            var aggregatedDirectory = Directory.CreateDirectory(Path.Combine(quarterlyReadingFilePath, sensor, "Aggregated")).FullName;

            //read the data from the given files
            var readings = CreateSensorReadingObjectsFromJSON(sensor);

            //sort the readings according to time stamps
            var sortedReadings = readings.OrderBy(x => x.ReadingTimestamp).ToList();

            List<AggregatedSensorReading> aggregatedReadingList = new List<AggregatedSensorReading>();

            int currentCount = 0;

            while (currentCount < sortedReadings.Count())
            {
                //initialise parameters
                DateTime currentDate = sortedReadings[currentCount].ReadingTimestamp;
                int numberOfValues = sortedReadings.Count(x => x.ReadingTimestamp.Date == currentDate);
                double[] measuringValues = new double[numberOfValues];
                DateTime[] timestamps = new DateTime[numberOfValues];

                //aggregate the data into a new object
                //while we are still within the same day, order the values into an array
                for (int i = 0; i < numberOfValues; i++)
                {
                    measuringValues[i] = sortedReadings[currentCount].ReadingValue;
                    timestamps[i] = sortedReadings[currentCount].ReadingTimestamp;
                    currentCount++;
                }

                //create an aggregated object
                var aggregatedReading = new AggregatedSensorReading()
                {
                    DeviceIdentifier = sortedReadings[currentCount - 1].DeviceIdentifier,
                    UnitOfMeasure = sortedReadings[currentCount - 1].UnitOfMeasure,
                    Id = Guid.NewGuid().ToString(),
                    TimestampSeries = timestamps,
                    MeasuringValues = measuringValues,
                    ReadingInterval = "daily",
                    ReadingTimestamp = currentDate,
                    ReadingValue = sortedReadings[currentCount - 1].ReadingValue //end of the day value
                };

                aggregatedReadingList.Add(aggregatedReading);
            }

            //save aggregates files
            foreach (var aggregateReading in aggregatedReadingList)
            {
                string timestampFormat = aggregateReading.TimestampSeries[0].ToString("yyyy-MM-dd_HH-mm");
                string fileName = timestampFormat + ".json";

                JSONFileUtils.PrettyWrite(aggregateReading, Path.Combine(aggregatedDirectory, fileName));
            }

        }

    }

    //inserting only 1 item
    private static async Task InsertSingleItem(Container container)
    {
        SensorReading sensorReading = GetSingleSensorReadingsToInsert(Path.Combine(filePath, "SingleValue"));

        //chose an item
        if (sensorReading.Id == null)
        {
            //object must contain a field called id!
            sensorReading.Id = Guid.NewGuid().ToString();
        }

        try
        {
            //create item and get request charge
            var response = await container.CreateItemAsync(sensorReading, new PartitionKey(sensorReading.DeviceIdentifier));

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Received  ({ex.Message})."); //{response.StatusCode}
        }

    }

    //using bulk executor library
    private static async Task BulkImportDataLoading(Container container)
    {
        try
        {
            //get the readings we want to insert
            Stopwatch stopwatch1 = Stopwatch.StartNew();
            var items = GetSensorReadingsToInsert();
            stopwatch1.Stop();

            int amountToInsert = items.Count;

            Console.WriteLine($"Generated {amountToInsert} items in {stopwatch1.Elapsed}.");

            Console.WriteLine($"Starting...");
            Stopwatch stopwatch2 = Stopwatch.StartNew();
            List<Task> tasks = new List<Task>(amountToInsert);

            foreach (var item in items)
            {
                tasks.Add(container.CreateItemAsync(item, new PartitionKey(item.DeviceIdentifier))
                    .ContinueWith(itemResponse =>
                    {
                        if (!itemResponse.IsCompletedSuccessfully)
                        {

                            AggregateException innerExceptions = itemResponse.Exception.Flatten();
                            if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                            {
                                Console.WriteLine($"Received {cosmosException.StatusCode}.");
                            }
                            else
                            {
                                Console.WriteLine($"Exception {innerExceptions.InnerExceptions.FirstOrDefault()}.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"RequestCharge: {itemResponse.Result.RequestCharge}, Retries: {itemResponse.Result.Diagnostics.GetFailedRequestCount()}");
                        }

                    }));
            }

            // Wait until all are done
            await Task.WhenAll(tasks);
            // </ConcurrentTasks>
            stopwatch2.Stop();
            Console.WriteLine($"Finished writing {amountToInsert} items in {stopwatch2.Elapsed}.");

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }

    //query the aggregated time series of a specific device
    private static async Task QueryAggTimeSeriesData(Container container)
    {
        List<AggregatedSensorReading> sensorReadings = new List<AggregatedSensorReading>();

        string deviceId = "00aabb";
        string type = "daily";
        Console.WriteLine($"Querying {type} data from {deviceId}");

        //formulate the query, make sure to read all the results
        var query = new QueryDefinition(
            query: "SELECT * FROM sensorInput s WHERE  s.deviceIdentifier = @device AND s.ReadingInterval = @interval"
            ).WithParameter("@interval", type).WithParameter("@device", deviceId);

        using FeedIterator<AggregatedSensorReading> feed = container.GetItemQueryIterator<AggregatedSensorReading>(
            queryDefinition: query
        );

        FeedResponse<AggregatedSensorReading> response = null;

        int numberofItems = 0;
        double totalRequestCharge = 0;
        TimeSpan totalTime = TimeSpan.Zero;

        //get all resulting files
        while (feed.HasMoreResults)
        {
            response = await feed.ReadNextAsync();

            numberofItems += response.Count;
            totalRequestCharge += response.RequestCharge;
            totalTime += response.Diagnostics.GetClientElapsedTime();

            sensorReadings.AddRange(response.ToList());
        }
        Console.WriteLine($"Data points: {numberofItems}, Request Charge: {totalRequestCharge}, Time: {totalTime}");

        //sort the readings
        var sortedReadings = sensorReadings.OrderBy(x => x.ReadingTimestamp).ToList();
        Console.WriteLine($"Covered TimeSpan from: {sortedReadings[0].ReadingTimestamp} to {sortedReadings[sortedReadings.Count - 1].ReadingTimestamp}");
    }

    //query the single time series data
    private static async Task QueryTimeSeriesData(Container container)
    {
        List<SensorReading> sensorReadings = new List<SensorReading>();

        //specify the sensor
        string deviceId = "00aabb";
        string type = "quarterHourly";
        Console.WriteLine($"Querying {type} data from {deviceId}");

        //get all the resulting files
        var query = new QueryDefinition(
            query: "SELECT * FROM sensorInput s WHERE  s.deviceIdentifier = @device AND s.ReadingInterval = @interval"
            ).WithParameter("@interval", type).WithParameter("@device", deviceId);

        using FeedIterator<SensorReading> feed = container.GetItemQueryIterator<SensorReading>(
            queryDefinition: query
        );

        FeedResponse<SensorReading> response = null;

        int numberofItems = 0;
        double totalRequestCharge = 0;
        TimeSpan totalTime = TimeSpan.Zero;

        //retrieve them all
        while (feed.HasMoreResults)
        {
            response = await feed.ReadNextAsync();

            numberofItems += response.Count;
            totalRequestCharge += response.RequestCharge;
            totalTime += response.Diagnostics.GetClientElapsedTime();

            sensorReadings.AddRange(response.ToList());
        }
        Console.WriteLine($"Data points: {numberofItems}, Request Charge: {totalRequestCharge}, Time: {totalTime}");

        //sort the readings
        var sortedReadings = sensorReadings.OrderBy(x => x.ReadingTimestamp).ToList();
        Console.WriteLine($"Covered TimeSpan from: {sortedReadings[0].ReadingTimestamp} to {sortedReadings[sortedReadings.Count - 1].ReadingTimestamp}");
    }

    //get all the readings from a specified sub directory
    private static List<SensorReading> GetSensorReadingsToInsert()
    {

        List<SensorReading> sensorReadings = new List<SensorReading>();
        //get the subdirectories (each corresponding to one sensor) from the filePath
        string[] readingIntervalDirectories = Directory.GetDirectories(filePath);

        foreach (string RIDirectory in readingIntervalDirectories)
        {
            string[] sensorDirectories = Directory.GetDirectories(RIDirectory);

            foreach (string counterDirectory in sensorDirectories)
            {
                var readings = CreateSensorReadingObjectsFromJSON(counterDirectory);

                sensorReadings.AddRange(readings);
            }
        }

        return sensorReadings;
    }


    /*private static List<AggregatedSensorReading> CreateAggSensorReadingObjectsFromJSON(string directory)
    {
        List<AggregatedSensorReading> sensorReadings = new List<AggregatedSensorReading>();

        //Get all json files from the directory
        string[] sensorFileNames = Directory.GetFiles(directory, "*.json");

        var jsonDocumentArray = new StringBuilder();
        jsonDocumentArray.Append("[");
        jsonDocumentArray.Append(File.ReadAllText(sensorFileNames[0]));

        for (int i = 1; i < sensorFileNames.Length; i++)
        {
            jsonDocumentArray.Append(", " + File.ReadAllText(sensorFileNames[i]));
        }

        jsonDocumentArray.Append("]");

        var sensorReading = JsonConvert.DeserializeObject<IList<AggregatedSensorReading>>(jsonDocumentArray.ToString());

        foreach (var r in sensorReading)
        {
            r.Id = Guid.NewGuid().ToString();
            sensorReadings.Add(r);
        }

        return sensorReadings;
    }*/

    //form a sensor reading object from read json files
    private static List<SensorReading> CreateSensorReadingObjectsFromJSON(string directory)
    {
        List<SensorReading> sensorReadings = new List<SensorReading>();

        //Get all json files from the directory
        string[] sensorFileNames = Directory.GetFiles(directory, "*.json");

        var jsonDocumentArray = new StringBuilder();
        jsonDocumentArray.Append("[");
        jsonDocumentArray.Append(File.ReadAllText(sensorFileNames[0]));

        for (int i = 1; i < sensorFileNames.Length; i++)
        {
            jsonDocumentArray.Append(", " + File.ReadAllText(sensorFileNames[i]));
        }

        jsonDocumentArray.Append("]");

        var sensorReading = JsonConvert.DeserializeObject<IList<SensorReading>>(jsonDocumentArray.ToString());

        foreach (var r in sensorReading)
        {
            r.Id = Guid.NewGuid().ToString();
            sensorReadings.Add(r);
        }

        return sensorReadings;
    }

    private static SensorReading GetSingleSensorReadingsToInsert(string singleValuePath)
    {

        string[] sensorFileNames = Directory.GetFiles(singleValuePath, "*.json");

        //Get a single json file
        var jsonText = File.ReadAllText(sensorFileNames[0]);

        var sensorReading = JsonConvert.DeserializeObject<SensorReading>(jsonText.ToString());
        sensorReading.Id = Guid.NewGuid().ToString();

        return sensorReading;
    }

}