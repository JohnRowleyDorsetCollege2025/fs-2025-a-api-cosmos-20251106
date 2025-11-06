using fs_2025_a_api_cosmos_20251106.Models;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);


var configuration = builder.Configuration;
var cosmosDbEndpoint = configuration["CosmosDb:EndpointUri"];
var cosmosDbKey = configuration["CosmosDb:PrimaryKey"];

var client= new Microsoft.Azure.Cosmos.CosmosClient(cosmosDbEndpoint, cosmosDbKey);
Console.WriteLine("Connected to Cosmos DB");

var database = await client.CreateDatabaseIfNotExistsAsync(configuration["CosmosDb:DatabaseName"]);

var container = await database.Database.CreateContainerIfNotExistsAsync(
    new Microsoft.Azure.Cosmos.ContainerProperties
    {
        Id = configuration["CosmosDb:ContainerName"],
        PartitionKeyPath = "/id"
    });

builder.Services.AddSingleton(client);
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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.MapGet("/insert", async (Container container) =>
{
    var student = new Student
    {
        id = Guid.NewGuid().ToString(),
        Name = "John Doe",
        Year = 2
    };
    var response = await container.CreateItemAsync(student, new PartitionKey(student.id));
    return Results.Ok(response.Resource);
});

app.MapGet("/students/{id}", async (string id, Container container) =>
{
    try
    {
        var response = await container.ReadItemAsync<Student>(id, new PartitionKey(id));
        return Results.Ok(response.Resource);
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }
});


app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
