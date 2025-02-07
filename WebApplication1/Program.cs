using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.AddConsole(); // Add Console logging provider
builder.Logging.SetMinimumLevel(LogLevel.Debug); // Set minimum log level to Debug
// Add rate limer and compression
builder.Services.AddResponseCompression(options =>
{
  options.EnableForHttps = true; // Enable compression for HTTPS requests
  options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
    new[] { "application/octet-stream" }); // Add additional MIME types
});

// Configure the problem detail
builder.Services.AddProblemDetails();
// Configure HttpLogging
builder.Services.AddHttpLogging(options =>
{
  options.LoggingFields = HttpLoggingFields.All; // Log all fields
});
builder.Logging.AddFilter(
"Microsoft.AspNetCore.HttpLogging", LogLevel.Information);

// building after configuring the builder instance
var app = builder.Build();

// Configure the problem detail in JSON (for when your app is Restful API only)
app.UseStatusCodePages();

// Configure the HTTP request pipeline | development
if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  app.UseHttpLogging(); // Enable HttpLogging in development
}
else
{
  app.UseExceptionHandler();
  app.UseResponseCompression(); // Enable compression for production
  app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
}


// Routing and CRUD operations | development
app.MapGet("/error", void () => throw new Exception());
app.MapGet("/fruit", () => Fruit.All);

var getFruit = (string id) => Fruit.All[id];
app.MapGet("/fruit/{id}", (string id) =>
{
  getFruit(id);
  return TypedResults.Ok(Fruit.All[id]);
});

app.MapPost("/fruit/{id}", (string id) =>
{
  return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            { "Username", new[] { "Username is required." ,"trust me","joebiden"}}
        });

}); // C

Handlers handlers = new();
app.MapPut("/fruit/{id}", handlers.ReplaceFruit);

app.MapDelete("/fruit/{id}", DeleteFruit);
app.Run();


static void DeleteFruit(string id)
{


  Fruit.All.Remove(id);
}

// creating a fruit record that has STATIC dictionary belonging to the Fruit not to an instance of fruit
record Fruit(string Name, int Stock)
{
  // key = string , value = Fruit
  public static readonly Dictionary<string, Fruit> All = new();
}

// class with two replace and create functions (crud)
class Handlers
{
  public void ReplaceFruit(string id, Fruit fruit)
  {
    Fruit.All[id] = fruit;
  }

  public static void AddFruit(string id, Fruit fruit)
  {
    Fruit.All.Add(id, fruit);
  }
}
