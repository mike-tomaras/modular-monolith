using Incepted.Domain.Companies.Application;
using Incepted.Domain.Deals.Application;
using Incepted.Files.AzStorage;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Incepted.Files;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    private const string localDevConnectionString = "DefaultEndpointsProtocol=https;" +
                                                    "AccountName=devstoreaccount1;" +
                                                    "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;" +
                                                    "BlobEndpoint=https://127.0.0.1:10000/devstoreaccount1;";

    public static void With(this IServiceCollection services)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        if (environmentName == "Development")
        {
            services.AddScoped<IDealFileService, BlobFileService>(provider => new BlobFileService(localDevConnectionString));
            services.AddScoped<ICompanyFileService, BlobFileService>(provider => new BlobFileService(localDevConnectionString));
        }
        else
        {
            var connString = Environment.GetEnvironmentVariable("DealFiles__StorageConnString");
            services.AddScoped<IDealFileService, BlobFileService>(provider => new BlobFileService(connString));
            services.AddScoped<ICompanyFileService, BlobFileService>(provider => new BlobFileService(connString));
        }
    }
}
