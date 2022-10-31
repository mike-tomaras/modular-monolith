using Incepted.Db.DataModels.CompanyDMs;
using Incepted.Db.DataSeeding.Company;
using Incepted.Shared.Enums;

namespace Incepted.Db.DataSeeding.SeedPrograms;

internal static class SeedLocalDevCompanies
{
    public static async Task Run()
    {
        var companies = new List<CompanyDM>
        {
            CompanyCreationUtils
            .CreateCompany(CompanyType.Broker)
            .WithName("Marsh")
            .WithRandomEmployees()
            .WithDevEmployees(),

            CompanyCreationUtils
            .CreateCompany(CompanyType.Insurer)
            .WithName("Riskpoint")
            .WithRandomEmployees()
            .WithDevEmployees(),

            CompanyCreationUtils
            .CreateCompany(CompanyType.Insurer)
            .WithName("Liberty")
            .WithRandomEmployees(),

            CompanyCreationUtils
            .CreateCompany(CompanyType.Insurer)
            .WithName("AIG")
            .WithRandomEmployees()
        };

        await companies.SaveAsync();
    }
}
