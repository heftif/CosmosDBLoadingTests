# CosmosDBLoadingTests
CosmosDB Loading Tests for Sensor Data with Web Visualization (Blazor)

## CosmosDB_MVC
Showing data from sensors in a ASP.NET MVC web application in a list. Allowing the user to see details, edit and delete data, as well as adding further data points. Data is retrieved from Azure Cosmos DB and stored there again.

### Installation
Add your Cosmos DB Account, Key, Database Name and Container Name to a file called ".env". Create the Database on your Azure Cosmos DB account. "id" is the identifier assigned by Cosmos DB, "identifier" is the partition key. 

## LoadTest_CosmosDB
.net Core Application that creates random sensor data and saves it in a locally specified folder. Then, the user can chose to bulk upload the data to Cosmos DB. After, user can query the data in Cosmos DB, getting time series from specified sensors.

### Installation
Add your Cosmos DB Account, Key, Database Name and Container Name to a file called ".env". Create the Database on your Azure Cosmos DB account. "id" is the identifier assigned by Cosmos DB, "identifier" is the partition key. Specify the local file path to store the randomly generated sensor data.
