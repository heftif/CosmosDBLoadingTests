using Microsoft.AspNetCore.Mvc;

namespace CosmosDB_MVC.Controllers
{
	using System;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Mvc;
	using CosmosDB_MVC.Services;
	using CosmosDB_MVC.Models;

	public class SensorReadingController : Controller
	{
		private readonly ICosmosDbService _cosmosDbService;
		public SensorReadingController(ICosmosDbService cosmosDbService)
		{
			_cosmosDbService = cosmosDbService;
		}

		[ActionName("Index")]
		public async Task<IActionResult> Index()
		{
			return View(await _cosmosDbService.GetItemsAsync("SELECT * FROM c OFFSET 1000 LIMIT 100"));
			//return View(await _cosmosDbService.GetItemsAsync("SELECT * FROM c where c.deviceIdentifier = \"6c261ea4-4363-4065-8782-201ced74cdbb\" OFFSET 0 LIMIT 200"));
		}

		[ActionName("Create")]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ActionName("Create")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> CreateAsync([Bind("Id,DeviceIdentifier,ReadingInterval,ReadingTimestamp")] SensorReading sensorReading)
		{
			if (ModelState.IsValid)
			{
				sensorReading.Id = Guid.NewGuid().ToString();
				await _cosmosDbService.AddItemAsync(sensorReading);
				return RedirectToAction("Index");
			}

			return View(sensorReading);
		}

		[HttpPost]
		[ActionName("Edit")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> EditAsync([Bind("Id,DeviceIdentifier,ReadingInterval,ReadingTimestamp")] SensorReading sensorReading)
		{
			if (ModelState.IsValid)
			{
				await _cosmosDbService.UpdateItemAsync(sensorReading.DeviceIdentifier, sensorReading);
				return RedirectToAction("Index");
			}

			return View(sensorReading);
		}

		[ActionName("Edit")]
		public async Task<ActionResult> EditAsync(string deviceId, string id)
		{
			if (deviceId == null)
			{
				return BadRequest();
			}

			SensorReading item = await _cosmosDbService.GetItemAsync(id, deviceId);
			if (item == null)
			{
				return NotFound();
			}

			return View(item);
		}

		[ActionName("Delete")]
		public async Task<ActionResult> DeleteAsync(string deviceId, string id)
		{
			if (deviceId == null)
			{
				return BadRequest();
			}

			SensorReading sensorReading = await _cosmosDbService.GetItemAsync(id, deviceId);
			if (sensorReading == null)
			{
				return NotFound();
			}

			return View(sensorReading);
		}

		[HttpPost]
		[ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeleteConfirmedAsync([Bind("DeviceIdentifier", "Id")] string deviceId, string id)
		{
			await _cosmosDbService.DeleteItemAsync(id, deviceId);
			return RedirectToAction("Index");
		}

		[ActionName("Details")]
		public async Task<ActionResult> DetailsAsync(string deviceId, string id)
		{
			return View(await _cosmosDbService.GetItemAsync(id, deviceId));
		}
	}
}
