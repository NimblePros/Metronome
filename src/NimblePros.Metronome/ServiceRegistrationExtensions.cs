using Microsoft.Extensions.DependencyInjection;

namespace NimblePros.Metronome;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddMetronomeServices(this IServiceCollection services)
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
}
