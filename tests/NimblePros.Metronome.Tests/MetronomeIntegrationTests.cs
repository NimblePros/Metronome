using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit;

namespace NimblePros.Metronome.Tests;

/// <summary>
/// Runs a minimal API app (no Identity, so nothing else registers IHttpContextAccessor) on a TestServer.
/// </summary>
public sealed class MetronomeIntegrationTests : IAsyncLifetime
{
  private const string StubClient = "stub";

  private readonly SqliteConnection _sqlite = new("Data Source=:memory:");
  private readonly FakeLogCollector _logs = new();
  private WebApplication _app = default!;
  private HttpClient _client = default!;

  public async Task InitializeAsync()
  {
    await _sqlite.OpenAsync();

    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();
    builder.Logging.ClearProviders();
    builder.Logging.AddProvider(new FakeLoggerProvider(_logs));

    builder.Services.AddMetronome();
    builder.Services.AddDbContext<TestDbContext>((provider, options) =>
      options.UseSqlite(_sqlite).AddMetronomeDbTracking(provider));
    builder.Services.AddHttpClient(StubClient, c => c.BaseAddress = new Uri("http://stub.test/"))
      .ConfigurePrimaryHttpMessageHandler(() => new StubHandler())
      .AddMetronomeHandler();

    _app = builder.Build();
    _app.UseMetronomeLoggingMiddleware();

    _app.MapGet("/none", () => "ok");
    _app.MapGet("/http/{count:int}", async (int count, IHttpClientFactory factory) =>
    {
      var client = factory.CreateClient(StubClient);
      await Task.WhenAll(Enumerable.Range(0, count).Select(_ => client.GetAsync("ping")));
      return "ok";
    });
    _app.MapGet("/db", async (TestDbContext db) =>
    {
      db.Items.Add(new Item { Name = "one" });
      await db.SaveChangesAsync();
      return await db.Items.CountAsync();
    });

    await _app.StartAsync();
    _client = _app.GetTestClient();

    using var scope = _app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<TestDbContext>().Database.EnsureCreatedAsync();
  }

  public async Task DisposeAsync()
  {
    _client.Dispose();
    await _app.DisposeAsync();
    await _sqlite.DisposeAsync();
  }

  private static Dictionary<string, string> ParseHeader(HttpResponseMessage response)
  {
    var value = Assert.Single(response.Headers.GetValues(CombinedCallLoggingMiddleware.HeaderName));
    Assert.Matches(new Regex(@"^DbCallCount=\d+;DbCallDuration=[\d.E+-]+ms;HttpCallCount=\d+;HttpCallDuration=[\d.E+-]+ms$"), value);
    return value.Split(';').Select(p => p.Split('=')).ToDictionary(p => p[0], p => p[1]);
  }

  [Fact]
  public async Task Request_WithNoCalls_AddsZeroCountHeaderAndLogsNothing()
  {
    var response = await _client.GetAsync("/none");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var header = ParseHeader(response);
    Assert.Equal("0", header["DbCallCount"]);
    Assert.Equal("0", header["HttpCallCount"]);
    Assert.DoesNotContain(_logs.GetSnapshot(), r => r.Category == typeof(CombinedCallLoggingMiddleware).FullName);
  }

  [Fact]
  public async Task ConcurrentHttpCalls_AreAllCountedInHeaderAndLog()
  {
    var response = await _client.GetAsync("/http/50");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("50", ParseHeader(response)["HttpCallCount"]);

    var log = Assert.Single(_logs.GetSnapshot(), r => r.Message.StartsWith("HTTP calls"));
    Assert.Equal(LogLevel.Information, log.Level);
    Assert.Equal("50", log.GetStructuredStateValue("HttpCallCount"));
  }

  [Fact]
  public async Task EfCoreCalls_AreCountedInHeaderAndLog()
  {
    var response = await _client.GetAsync("/db");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var header = ParseHeader(response);
    Assert.Equal("2", header["DbCallCount"]); // the INSERT and the COUNT
    Assert.Equal("0", header["HttpCallCount"]);

    var log = Assert.Single(_logs.GetSnapshot(), r => r.Message.StartsWith("Database calls"));
    Assert.Equal("2", log.GetStructuredStateValue("DbCallCount"));
  }

  [Fact]
  public async Task Counts_AreScopedToEachRequest()
  {
    await _client.GetAsync("/http/3");
    var response = await _client.GetAsync("/http/1");

    Assert.Equal("1", ParseHeader(response)["HttpCallCount"]);
  }

  [Fact]
  public async Task HttpCall_OutsideARequest_IsSentWithoutTracking()
  {
    var factory = _app.Services.GetRequiredService<IHttpClientFactory>();

    var response = await factory.CreateClient(StubClient).GetAsync("ping");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public void FormatHeader_UsesInvariantCulture()
  {
    var original = CultureInfo.CurrentCulture;
    CultureInfo.CurrentCulture = new CultureInfo("de-DE");
    try
    {
      var db = new DbCallCounter();
      db.RecordCall(TimeSpan.FromMilliseconds(1.5));
      var http = new HttpCallCounter();
      http.RecordCall(TimeSpan.FromMilliseconds(2.25));

      Assert.Equal("DbCallCount=1;DbCallDuration=1.5ms;HttpCallCount=1;HttpCallDuration=2.25ms",
        CombinedCallLoggingMiddleware.FormatHeader(db, http));
    }
    finally
    {
      CultureInfo.CurrentCulture = original;
    }
  }

  private sealed class StubHandler : HttpMessageHandler
  {
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      await Task.Yield();
      return new HttpResponseMessage(HttpStatusCode.OK);
    }
  }

  public sealed class Item
  {
    public int Id { get; set; }
    public string Name { get; set; } = "";
  }

  public sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
  {
    public DbSet<Item> Items => Set<Item>();
  }
}
