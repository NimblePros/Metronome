using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NimblePros.Metronome;

public class CombinedCallLoggingMiddleware
{
  /// <summary>
  /// The name of the response header that holds the call counts and durations for the request.
  /// </summary>
  public const string HeaderName = "NimblePros.Metronome";

  private readonly RequestDelegate _next;

  public CombinedCallLoggingMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(
      HttpContext context,
      DbCallCounter dbCounter,
      HttpCallCounter httpCounter,
      ILogger<CombinedCallLoggingMiddleware> logger)
  {
    context.Response.OnStarting(() =>
    {
      context.Response.Headers[HeaderName] = FormatHeader(dbCounter, httpCounter);
      return Task.CompletedTask;
    });

    await _next(context);

    if (dbCounter.CallCount > 0)
    {
      logger.LogInformation("Database calls: {DbCallCount}, Total time: {DbCallDuration} ms",
        dbCounter.CallCount, dbCounter.TotalTimeSpan.TotalMilliseconds);
    }
    if (httpCounter.CallCount > 0)
    {
      logger.LogInformation("HTTP calls: {HttpCallCount}, Total time: {HttpCallDuration} ms",
        httpCounter.CallCount, httpCounter.TotalTimeSpan.TotalMilliseconds);
    }
  }

  /// <summary>
  /// Formats the header value, for example <c>DbCallCount=2;DbCallDuration=6.4678ms;HttpCallCount=1;HttpCallDuration=222.3137ms</c>.
  /// Numbers always use the invariant culture so the value parses the same way everywhere.
  /// </summary>
  internal static string FormatHeader(DbCallCounter dbCounter, HttpCallCounter httpCounter) =>
    string.Create(CultureInfo.InvariantCulture,
      $"DbCallCount={dbCounter.CallCount};DbCallDuration={dbCounter.TotalTimeSpan.TotalMilliseconds}ms;HttpCallCount={httpCounter.CallCount};HttpCallDuration={httpCounter.TotalTimeSpan.TotalMilliseconds}ms");
}
