using Incepted.Domain.Companies.Application;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Incepted.Domain.Companies;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    public static void With(this IServiceCollection services)
    {
        services.AddScoped<ICompanyService, CompanyService>();
    }
}
