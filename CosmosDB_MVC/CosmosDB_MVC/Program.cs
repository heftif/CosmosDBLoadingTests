using CosmosDB_MVC;
using CosmosDB_MVC.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

//define environment variable
var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Configuration.AddEnvironmentVariables();
builder.Services.AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync().GetAwaiter().GetResult());

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
static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync()
{
	string databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");
    string containerName = Environment.GetEnvironmentVariable("CONTAINER_NAME");
    string account = Environment.GetEnvironmentVariable("ACCOUNT");
    string key = Environment.GetEnvironmentVariable("KEY") + "==";
    Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
	CosmosDbService cosmosDbService = new CosmosDbService(client, databaseName, containerName);
	//Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
	//await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

	return cosmosDbService;
}