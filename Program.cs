using Microsoft.Azure.Cosmos;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

var endpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT")
    ?? builder.Configuration["CosmosDb:AccountEndpoint"];
var key = Environment.GetEnvironmentVariable("COSMOS_KEY")
    ?? builder.Configuration["CosmosDb:AccountKey"];
var dbName = Environment.GetEnvironmentVariable("COSMOS_DB")
    ?? builder.Configuration["CosmosDb:DatabaseName"]
    ?? "CatalogDb";
var containerName = Environment.GetEnvironmentVariable("COSMOS_CONTAINER")
    ?? builder.Configuration["CosmosDb:ContainerName"]
    ?? "Products";

if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(key))
{
    throw new InvalidOperationException(
        "Faltan credenciales Cosmos. Configura COSMOS_ENDPOINT/COSMOS_KEY o CosmosDb:AccountEndpoint/CosmosDb:AccountKey en appsettings.");
}

builder.Services.AddSingleton(new CosmosClient(endpoint, key, new CosmosClientOptions
{
    ConnectionMode = ConnectionMode.Gateway,
    LimitToEndpoint = true,
    HttpClientFactory = () =>
    {
        var isLocalEndpoint = endpoint.Contains("localhost", StringComparison.OrdinalIgnoreCase)
            || endpoint.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);

        if (!isLocalEndpoint)
        {
            return new HttpClient();
        }

        // Solo para Cosmos Emulator en local: evita fallo SSL por cert autofirmado.
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (HttpRequestMessage _, X509Certificate2? _, X509Chain? _, SslPolicyErrors _) => true
        };

        return new HttpClient(handler);
    }
}));

var app = builder.Build();

var client = app.Services.GetRequiredService<CosmosClient>();
var database = await client.CreateDatabaseIfNotExistsAsync(dbName);

// Importante: usa una partition key de negocio, no /id
var container = await database.Database.CreateContainerIfNotExistsAsync(
    new ContainerProperties(containerName, "/category")
);

app.MapGet("/", () => Results.Ok("API .NET + Cosmos DB funcionando"));

app.MapGet("/products/{id}/{category}", async (string id, string category) =>
{
    try
    {
        var response = await container.Container.ReadItemAsync<Product>(id, new PartitionKey(category));
        return Results.Ok(response.Resource);
    }
    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound("Producto no encontrado.");
    }
});

app.MapGet("/products/by-category/{category}", async (string category) =>
{
    var query = container.Container.GetItemQueryIterator<Product>(
        new QueryDefinition("SELECT * FROM c WHERE c.category = @category")
            .WithParameter("@category", category)
    );

    var items = new List<Product>();
    while (query.HasMoreResults)
    {
        var page = await query.ReadNextAsync();
        items.AddRange(page);
    }

    return Results.Ok(items);
});

app.MapPost("/products", async (Product input) =>
{
    var id = string.IsNullOrWhiteSpace(input.id) ? Guid.NewGuid().ToString() : input.id;
    var item = input with { id = id };

    try
    {
        var created = await container.Container.CreateItemAsync(item, new PartitionKey(item.category));
        return Results.Created($"/products/{created.Resource.id}/{created.Resource.category}", created.Resource);
    }
    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
    {
        return Results.Conflict("Ya existe un producto con ese id en esa partición.");
    }
});

app.MapPut("/products/{id}/{category}", async (string id, string category, Product input) =>
{
    var item = input with { id = id, category = category };

    try
    {
        await container.Container.ReplaceItemAsync(item, id, new PartitionKey(category));
        return Results.NoContent();
    }
    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound("Producto no encontrado.");
    }
});

app.MapDelete("/products/{id}/{category}", async (string id, string category) =>
{
    try
    {
        await container.Container.DeleteItemAsync<Product>(id, new PartitionKey(category));
        return Results.NoContent();
    }
    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound("Producto no encontrado.");
    }
});

app.Run();

public record Product(string id, string name, string category, decimal price);
