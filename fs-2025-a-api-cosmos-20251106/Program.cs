using fs_2025_a_api_cosmos_20251106.Models;
using Microsoft.Azure.Cosmos;
using System.Net;

var builder = WebApplication.CreateBuilder(args);


var configuration = builder.Configuration;
var cosmosDbEndpoint = configuration["CosmosDb:EndpointUri"];
var cosmosDbKey = configuration["CosmosDb:PrimaryKey"];

var client= new Microsoft.Azure.Cosmos.CosmosClient(cosmosDbEndpoint, cosmosDbKey);
Console.WriteLine("Connected to Cosmos DB");

var database = await client.CreateDatabaseIfNotExistsAsync(configuration["CosmosDb:DatabaseName"]);

var containerResponse = await database.Database.CreateContainerIfNotExistsAsync(
    new Microsoft.Azure.Cosmos.ContainerProperties
    {
        Id = "orders",
        PartitionKeyPath = "/id"
    });

var container = containerResponse.Container;

//builder.Services.AddSingleton(container.Container);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();





// Create/insert an order
app.MapPost("/orders", async (Order order) =>
{
    if (order is null || order.Customer is null || string.IsNullOrWhiteSpace(order.Customer.CustomerId))
        return Results.BadRequest("Order must include customer with customerId.");

    // Ensure an id if caller didn't send one
    order.id ??= $"order-{Guid.NewGuid()}";

    var result = await container.CreateItemAsync(order, new PartitionKey(order.id));
    return Results.Created($"/orders/{order.id}", result.Resource);
})
.WithName("CreateOrder");



// Get one order by partition (customerId) + id (fast point-read)
app.MapGet("/orders/{id}", async (string id) =>
{
    try
    {
        var response = await container.ReadItemAsync<Order>(id, new PartitionKey(id));

       Console.WriteLine(response.Resource.Customer.Name);
        return Results.Ok(response.Resource);
    }
    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }
})
.WithName("GetOrder");


app.Run();


