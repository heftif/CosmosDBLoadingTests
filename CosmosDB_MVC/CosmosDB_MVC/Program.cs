using CosmosDB_MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var cosmosDBSetting = builder.Configuration.GetSection("CosmosDb");
builder.Services.AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync(cosmosDBSetting).GetAwaiter().GetResult());

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=SensorReading}/{action=Index}/{id?}");

app.Run();


/// <summary>
/// Initialise a Cosmos DB database. 
/// </summary>
/// <returns></returns>
static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection)
{
	string databaseName = configurationSection.GetSection("DatabaseName").Value;
	string containerName = configurationSection.GetSection("ContainerName").Value;
	string account = configurationSection.GetSection("Account").Value;
	string key = configurationSection.GetSection("Key").Value;
	Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
	CosmosDbService cosmosDbService = new CosmosDbService(client, databaseName, containerName);
	//Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
	//await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

	return cosmosDbService;
}