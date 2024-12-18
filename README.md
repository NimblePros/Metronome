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
        .AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>())
    );
```

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

When hitting individual pages or endpoints, you should see the following output in the console:

```
info: NimblePros.Metronome.CombinedCallLoggingMiddleware[0]
      Database calls: 2, Total time: 6.4678 ms
info: NimblePros.Metronome.CombinedCallLoggingMiddleware[0]
      HTTP calls: 1, Total time: 222.3137 ms
```

You can use this to identify slow pages or endpoints and optimize them by adding caching, combining queries, or reducing the number of HTTP calls.

If you need help doing this in an effective way across your entire application, [contact NimblePros](https://nimblepros.com). We do this all the time.
