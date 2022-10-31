using Incepted.Db.DataModels.CompanyDMs;
using Incepted.Db.DataSeeding.Company;
using Microsoft.Azure.Cosmos;

namespace Incepted.Db.DataSeeding.Company;

internal static class CompanyPersistenceUtils
{
    private static Container Container =>
        new CosmosClient(ConnStrings.LocalDevConnectionString)
        .GetDatabase(id: "portaldb")
        .GetContainer(id: "companies");

    public static async Task SaveAsync(this CompanyDM company)
    {
        Console.Write("Saving company...");
        await Container.Upsert(company);
        Console.WriteLine("DONE");
    }

    public static async Task SaveAsync(this IEnumerable<CompanyDM> companies)
    {
        Console.Write("Saving companies...");
        var container = Container;

        await Task.WhenAll(companies.Select(container.Upsert));
        Console.WriteLine("DONE");
    }

    private static async Task Upsert(this Container container, CompanyDM company)
    {
        if (!company.Employees.Any())
        {
            Console.WriteLine($"ERROR: No employees in company with Id '{company.Id}' and Name '{company.Name}'. Use one of the 'With*Employees()' methods to populate them.");
            return;
        }

        await container.UpsertItemAsync(
            item: company,
            partitionKey: new PartitionKey(company.PartitionKey_CompanyId.ToString())
        );

        var employees = company.Employees
            .Select(EmployeeDM.Factory.ToEntity)
            .Select(e => EmployeeLookupDM.Factory.ToDataModel(e, company.Id));
        foreach (var employee in employees)
        {
            await container.UpsertItemAsync(
                        item: employee,
                        partitionKey: new PartitionKey(employee.PartitionKey_CompanyId.ToString())
                    );
        }
    }
}
