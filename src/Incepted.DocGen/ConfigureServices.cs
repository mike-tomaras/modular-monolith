using Incepted.DocGen.SyncFusion;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Incepted.DocGen;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    public static void With(this IServiceCollection services)
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Njc3MjEzQDMyMzAyZTMyMmUzMG5yaytvdDVQVXU3V1BoNnZOVGlRVUlYK3VrNU0vd0h5KzFDZXVDZ3lNa2c9");

        services.AddScoped<IDocGenService, SyncFusionDocGeneratorService>();
    }
}
