using Incepted.Domain.Deals.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Incepted.Domain.Deals;

public static class ConfigureServices
{
    public static void With(this IServiceCollection services)
    {
        services.AddScoped<IDealService, DealService>();
    }
}
