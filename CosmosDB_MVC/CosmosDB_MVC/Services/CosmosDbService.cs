namespace CosmosDB_MVC.Services
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using CosmosDB_MVC.Models;
	using Microsoft.Azure.Cosmos;


	public class CosmosDbService : ICosmosDbService
	{
		private Container _container;

		public CosmosDbService(
			CosmosClient dbClient,
			string databaseName,
			string containerName)
		{
			this._container = dbClient.GetContainer(databaseName, containerName);
		}

		public async Task AddItemAsync(SensorReading reading)
		{
			await this._container.CreateItemAsync<SensorReading>(reading, new PartitionKey(reading.DeviceIdentifier));
		}

		public async Task DeleteItemAsync(string id, string deviceId)
		{
			await this._container.DeleteItemAsync<SensorReading>(id, new PartitionKey(deviceId));
		}

		public async Task<SensorReading> GetItemAsync(string id, string deviceId)
		{
			try
			{
				ItemResponse<SensorReading> response = await this._container.ReadItemAsync<SensorReading>(id, new PartitionKey(deviceId));
				return response.Resource;
			}
			catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return null;
			}

		}

		public async Task<IEnumerable<SensorReading>> GetItemsAsync(string queryString)
		{
			var query = this._container.GetItemQueryIterator<SensorReading>(new QueryDefinition(queryString));
			List<SensorReading> results = new List<SensorReading>();
			while (query.HasMoreResults)
			{
				var response = await query.ReadNextAsync();

				results.AddRange(response.ToList());
			}

			return results;
		}

		public async Task UpdateItemAsync(string deviceId, SensorReading reading)
		{
			await this._container.UpsertItemAsync<SensorReading>(reading, new PartitionKey(deviceId));
		}
	}
}

