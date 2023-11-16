using CosmosDB_MVC.Models;

namespace CosmosDB_MVC.Services
{
	public interface ICosmosDbService
	{
		Task<IEnumerable<SensorReading>> GetItemsAsync(string query);
		Task<SensorReading> GetItemAsync(string id, string deviceId);
		Task AddItemAsync(SensorReading reading);
		Task UpdateItemAsync(string deviceId, SensorReading reading);
		Task DeleteItemAsync(string id, string deviceId);
	}
}
