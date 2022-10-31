using Incepted.Db.DataModels.CompanyDMs;
using Incepted.Db.DataModels.SharedDMs;
using Incepted.Db.DataSeeding.Company;
using Incepted.Shared.Enums;

namespace Incepted.Db.DataSeeding.SeedPrograms;

internal static class SeedProdCompanies
{
    public static async Task Run()
    {
        var konradIns2 = new EmployeeDM
        {
            Version = 1,
            Id = Guid.NewGuid(),
            Email = "konrad+insurer2@incepted.io",
            Name = new HumanNameDM { First = "KonradIns2", Last = "Rotthege" },
            UserId = "auth0|634a821162cb54b2e9165156"
        };
        var konradIns3 = new EmployeeDM
        {
            Version = 1,
            Id = Guid.NewGuid(),
            Email = "konrad+insurer3@incepted.io",
            Name = new HumanNameDM { First = "KonradIns3", Last = "Rotthege" },
            UserId = "auth0|634a817062cb54b2e916514c"
        };

        var companies = new List<CompanyDM>
        {
            CompanyCreationUtils
            .CreateCompany(CompanyType.Broker)
            .WithName("Marsh")
            .WithRandomEmployees()
            .WithProdEmployees(),

            CompanyCreationUtils
            .CreateCompany(CompanyType.Insurer)
            .WithName("Riskpoint")
            .WithRandomEmployees()
            .WithProdEmployees(),

            CompanyCreationUtils
            .CreateCompany(CompanyType.Insurer)
            .WithName("Liberty")
            .WithEmployee(konradIns2)
            .WithRandomEmployees(),

            CompanyCreationUtils
            .CreateCompany(CompanyType.Insurer)
            .WithName("AIG")
            .WithEmployee(konradIns3)
            .WithRandomEmployees()
        };

        await companies.SaveAsync();
    }
}
