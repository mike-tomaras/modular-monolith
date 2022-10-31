using Incepted.Db.DataModels.CompanyDMs;
using Incepted.Domain.Companies.Application;
using Incepted.Domain.Companies.Entities;
using Incepted.Shared;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Optional;
using System.Collections.Immutable;

namespace Incepted.Db.Repos;

internal class CompanyRepo : ICompanyRepo
{
    private readonly ILogger<CompanyRepo> _logger;
    private readonly Container _container;

    public CompanyRepo(string connString, ILogger<CompanyRepo> logger)
    {
        var client = new CosmosClient(connString);
        Database database = client.GetDatabase(id: "portaldb");
        _container = database.GetContainer(id: "companies");
        _logger = logger;
    }

    public async Task<Option<IImmutableList<Company>, ErrorCode>> GetCompaniesOfType(CompanyType type)
    {
        var result = await _container.GetResultsAsync<CompanyDM>(
            query: $"SELECT * FROM c WHERE c.CompanyType = {(int)type} AND c.Type = 'company'"
            );

        _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "get companies", result.ruTotal);

        return result.values
            .Select(CompanyDM.Factory.ToEntity)
            .ToImmutable()
            .Some<IImmutableList<Company>, ErrorCode>();
    }

    public async Task<Option<Company, ErrorCode>> GetCompany(Guid id)
    {
        ItemResponse<CompanyDM> itemResponse = 
            await _container.ReadItemAsync<CompanyDM>(
                id: id.ToString(),
                partitionKey: new PartitionKey(id.ToString())
            );

        _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "get single company", itemResponse.RequestCharge);

        return CompanyDM.Factory.ToEntity(itemResponse.Resource).Some<Company, ErrorCode>();
    }

    public async Task<Option<Company, ErrorCode>> GetCompanyOfEmployee(UserId userId)
    {
        var query = $"SELECT * FROM c WHERE c.UserId = '{userId}' AND c.Type = 'employee-lookup'";
        var result = await _container.GetResultsAsync<EmployeeLookupDM>(query);
        var employeeLookup = result.values.FirstOrDefault();

        _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "get company of employee", result.ruTotal);

        if (employeeLookup == null) return Option.None<Company, ErrorCode>(CompanyErrorCodes.EmployeeNotFound);

        return await GetCompany(employeeLookup.PartitionKey_CompanyId);
    }

    public async Task<Option<Unit, ErrorCode>> Update(Company company)
    {
        var item = CompanyDM.Factory.ToDataModel(company);
        ItemResponse<CompanyDM> itemResponse = 
            await _container.ReplaceItemAsync(
                item: item,
                id: item.Id.ToString(),
                partitionKey: new PartitionKey(item.Id.ToString())
            );

        _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "update company", itemResponse.RequestCharge);

        return Option.Some<Unit, ErrorCode>(new Unit());
    }

    public async Task<Option<Unit, ErrorCode>> Update(Employee employee, Guid companyId)
    {
        double ruTotal = 0;

        var query = $"SELECT * FROM c WHERE c.companyId = '{companyId}' AND c.Type = 'company'";
        var result = await _container.GetResultsAsync<CompanyDM>(query);
        var company = result.values.FirstOrDefault();
        ruTotal += result.ruTotal;

        if (company == null) return Option.None<Unit, ErrorCode>(CompanyErrorCodes.CompanyNotFound);

        var employeeToUpdate = company.Employees.FirstOrDefault(e => e.Id == employee.Id);

        if (employeeToUpdate == null) return Option.None<Unit, ErrorCode>(CompanyErrorCodes.EmployeeNotFound);

        employeeToUpdate = EmployeeDM.Factory.ToDataModel(employee);
        ItemResponse<EmployeeDM> employeeItemResponse = 
            await _container.ReplaceItemAsync(
                item: employeeToUpdate,
                id: employeeToUpdate.Id.ToString(),
                partitionKey: new PartitionKey(companyId.ToString())
            );
        ruTotal += employeeItemResponse.RequestCharge;

        _logger.LogInformation("Action: {RuAction} RUs consumed: {RuConsumed}", "update employee", ruTotal);

        return Option.Some<Unit, ErrorCode>(new Unit());
    }
}