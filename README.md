# Metronome by NimblePros

A simple tool for tracking Database and Http calls per ASP.NET Core request.

## Installation

Install the NuGet package:

```powershell
Install-Package NimblePros.Metronome

dotnet add package NimblePros.Metronome
```

## Usage

Refer to the samples in the [samples](samples) directory.

### Wire up services

```csharp
builder.Services.AddMetronome();
```

### Add Middleware

Add the middleware near the top of the pipeline. Recommended to be used only in development environment.

```csharp
if (app.Environment.IsDevelopment())
{
  app.UseMetronomeLoggingMiddleware();
}
```

### EF Core Query Tracking

```csharp
builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
    options.UseSqlite(connectionString) // or any other provider
        .AddMetronomeDbTracking(provider)
    );
```

### Dapper Query Tracking

Inject the scoped `DbCallCounter` and pass it as the first argument of a Dapper call. Calls are counted with any EF Core calls in the same request.

```csharp
using NimblePros.Metronome;

public class OrdersService(IDbConnection connection, DbCallCounter counter)
{
  public Task<IEnumerable<Order>> ListAsync(int customerId) =>
    connection.QueryAsync<Order>(counter, "SELECT * FROM Orders WHERE CustomerId = @customerId", new { customerId });
}
```

Available with sync and async versions: `Query<T>`, `QueryFirst<T>`, `QueryFirstOrDefault<T>`, `QuerySingle<T>`, `QuerySingleOrDefault<T>`, `Execute`, and `ExecuteScalar<T>`. Other Dapper methods, such as multi-mapping queries and `QueryMultiple`, are not tracked.

### HttpClient Tracking

```csharp
// simple named client
builder.Services.AddHttpClient("Foo")
    .AddMetronomeHandler();

// strongly typed client
builder.Services.AddScoped<IPostsService, PostsService>(); // interface is optional
builder.Services.AddHttpClient<IPostsService, PostsService>()
    .ConfigureHttpClient(client =>
    {
      client.BaseAddress = new Uri("https://my-json-server.typicode.com/typicode/demo/");
    })
    .AddMetronomeHandler();
```

## Expected Output

### Log Output

When hitting individual pages or endpoints, you should see the following output in the console:

```
info: NimblePros.Metronome.CombinedCallLoggingMiddleware[0]
      Database calls: 2, Total time: 6.4678 ms
info: NimblePros.Metronome.CombinedCallLoggingMiddleware[0]
      HTTP calls: 1, Total time: 222.3137 ms
```

### HTTP Headers

In addition, the information is added to a response header (unless configured not to do so):

![image](https://github.com/user-attachments/assets/a2076ab9-0808-4515-976f-1e6a13951568)

## Using the Information

You can use this to identify slow pages or endpoints and optimize them by adding caching, combining queries, or reducing the number of HTTP calls.

If you need help doing this in an effective way across your entire application, [contact NimblePros](https://nimblepros.com). We do this all the time.
