using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NimblePros.Metronome;

public static class ServiceRegistrationExtensions
{
  public static IServiceCollection AddMetronome(this IServiceCollection services)
  {
    // The HTTP handler finds the request's counter through the current HttpContext.
    services.AddHttpContextAccessor();

    services.AddScoped<DbCallCountingInterceptor>();
    services.AddScoped<DbCallCounter>();

    // IHttpClientFactory creates handlers in its own scope and caches them, so they are transient.
    services.AddTransient<HttpCallCountingHandler>();
    services.AddScoped<HttpCallCounter>();

    return services;
  }

  public static IHttpClientBuilder AddMetronomeHandler(this IHttpClientBuilder builder)
  {
    return builder.AddHttpMessageHandler<HttpCallCountingHandler>();
  }

  /// <summary>
  /// Adds the interceptor that counts this request's database calls. Use it with
  /// <c>AddDbContext((provider, options) =&gt; ...)</c>, which builds the options in the request scope.
  /// It isn't supported with <c>AddDbContextPool</c> or <c>AddDbContextFactory</c>, whose options are
  /// built once and shared across requests.
  /// </summary>
  public static DbContextOptionsBuilder AddMetronomeDbTracking(this DbContextOptionsBuilder builder, IServiceProvider provider)
  {
    return builder.AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
  }
}
