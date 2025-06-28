using Linguana.Application.Interfaces;
using Linguana.Application.Services;

namespace Linguana.API.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISpeechService, SpeechService>();
        
        return services;
    }
}