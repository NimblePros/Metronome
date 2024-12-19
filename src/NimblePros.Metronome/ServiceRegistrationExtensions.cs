using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NimblePros.Metronome;

public static class ServiceRegistrationExtensions
{
  public static IServiceCollection AddMetronome(this IServiceCollection services)
  {
    services.AddScoped<DbCallCountingInterceptor>();
    services.AddScoped<DbCallCounter>();

    services.AddScoped<HttpCallCountingHandler>();
    services.AddScoped<HttpCallCounter>();

    return services;
  }

  public static IHttpClientBuilder AddMetronomeHandler(this IHttpClientBuilder builder)
  {
    return builder.AddHttpMessageHandler<HttpCallCountingHandler>();
  }

  public static DbContextOptionsBuilder AddMetronomeDbTracking(this DbContextOptionsBuilder builder, IServiceProvider provider)
  {
    return builder.AddInterceptors(provider.GetRequiredService<DbCallCountingInterceptor>());
  }
}
