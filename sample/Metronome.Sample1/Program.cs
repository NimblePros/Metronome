using MetricTesting2.Data;
using MetricTesting2.Pages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NimblePros.Metronome;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("SqliteConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
    options.UseSqlite(connectionString)
    .AddMetronomeDbTracking(provider)
    );
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

builder.Services.AddMetronome();

builder.Services.AddScoped<IPostsService, PostsService>();
builder.Services.AddHttpClient<IPostsService, PostsService>()
    .ConfigureHttpClient(client =>
    {
      client.BaseAddress = new Uri("https://my-json-server.typicode.com/typicode/demo/");
    })
    .AddMetronomeHandler();

builder.Services.AddHttpClient(); // TODO: configure the default client

builder.Services.AddHttpClient("Foo")
    .AddMetronomeHandler();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseMigrationsEndPoint();
  app.UseMetronomeLoggingMiddleware();
}
else
{
  app.UseExceptionHandler("/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapGet("/hello", async (IHttpClientFactory factory) =>
{
  var client = factory.CreateClient("Foo");
  client.BaseAddress = new Uri("https://my-json-server.typicode.com/typicode/demo/");
  await client.GetAsync("posts");
  return Results.Text("Hello World");
});


app.Run();

