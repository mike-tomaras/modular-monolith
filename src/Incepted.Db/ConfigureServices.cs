using Incepted.Db.Repos;
using Incepted.Domain.Companies.Application;
using Incepted.Domain.Companies.Repo;
using Incepted.Domain.Deals.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Incepted.Db;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    //cosmosdb emulator connstring is not a secret so we can add it here
    private const string localDevConnectionString = 
        "AccountEndpoint=https://localhost:8081/;" +
        "AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    public static void With(this IServiceCollection services)
    {
        services.AddScoped<IDealRepo, DevDealRepo>();
        services.AddScoped<ICompanyRepo, DevCompanyRepo>();

        //var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        //if (environmentName == "Development")
        //{
        //    services.AddScoped<IDealRepo, DealRepo>(provider => new DealRepo(localDevConnectionString, provider.GetRequiredService<ILogger<DealRepo>>()));
        //    services.AddScoped<ICompanyRepo, CompanyRepo>(provider => new CompanyRepo(localDevConnectionString, provider.GetRequiredService<ILogger<CompanyRepo>>()));
        //}
        //else
        //{
        //    var connString = Environment.GetEnvironmentVariable("dbConnString");
        //    services.AddScoped<IDealRepo, DealRepo>(provider => new DealRepo(connString, provider.GetRequiredService<ILogger<DealRepo>>()));
        //    services.AddScoped<ICompanyRepo, CompanyRepo>(provider => new CompanyRepo(connString, provider.GetRequiredService<ILogger<CompanyRepo>>()));
        //}
    }
}
