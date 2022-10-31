using Incepted.Domain.Companies.Application;
using Incepted.Domain.Companies.Entities;
using Incepted.Shared;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using Optional;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;

namespace Incepted.Domain.Companies.Repo;

internal class DevCompanyRepo : ICompanyRepo
{
    private static readonly CompanySeedData _companies = new CompanySeedData();

    public Task<Option<IImmutableList<Company>, ErrorCode>> GetCompaniesOfType(CompanyType type)
    {
        return Task.FromResult(_companies.Data
            .Where(c => c.Type == type)
            .ToImmutable()
            .Some<IImmutableList<Company>, ErrorCode>());
    }

    public Task<Option<Company, ErrorCode>> GetCompany(Guid id)
    {
        return Task.FromResult(_companies.Data
            .SingleOrDefault(c => c.Id == id)
            .SomeNotNull(CompanyErrorCodes.CompanyNotFound));
    }

    public Task<Option<Company, ErrorCode>> GetCompanyOfEmployee(UserId userId)
    {
        return Task.FromResult(_companies.Data
            .SingleOrDefault(c => c.Employees.Any(e => e.UserId == userId))
            .SomeNotNull(CompanyErrorCodes.CompanyNotFound));
    }

    public Task<Option<Unit, ErrorCode>> Update(Company company)
    {
        _companies.Data.ReplaceInList(c => c.Id == company.Id, company);

        return Task.FromResult(Option.Some<Unit, ErrorCode>(new Unit()));
    }

    public Task<Option<Unit, ErrorCode>> Update(Employee employee, Guid companyId)
    {
        //TODO: implement db
        

        return Task.FromResult(Option.Some<Unit, ErrorCode>(new Unit()));
    }
}

[ExcludeFromCodeCoverage]
public class CompanySeedData
{
    public List<Company> Data { get; set; }

    public CompanySeedData()
    {
        var brokers = new List<Employee> {
            new Employee(Guid.NewGuid(), new UserId("auth0|626e80056c48dc006a2de396"), new HumanName("Konrad", "Rotthege"), new MailAddress("konrad@broker.com")),
            new Employee(Guid.NewGuid(), new UserId("auth0|626e80401d742f006f2ab6a6"), new HumanName("Jamie", "Brown"), new MailAddress("jamie@broker.com")),
            new Employee(Guid.NewGuid(), new UserId("auth0|62492cd27810d5006999aa22"), new HumanName("Mike", "Tomaras"), new MailAddress("mike@broker.com")),
            new Employee(Guid.NewGuid(), new UserId("auth0|6255a0d5c0f77100691fba5c"), new HumanName("John", "Doe"), new MailAddress("john@broker.com")),
            new Employee(Guid.NewGuid(), new UserId("auth0|6255a0d5c0f77100691fba52"), new HumanName("Jane", "Doe"), new MailAddress("jane@broker.com")),
            new Employee(Guid.NewGuid(), new UserId("auth0|6255a0d5c0f77100691fba53"), new HumanName("Bob", "Squarepants"), new MailAddress("bob@broker.com")),
            new Employee(Guid.NewGuid(), new UserId("auth0|6255a0d5c0f77100691fba54"), new HumanName("Patrick", "Star"), new MailAddress("pat@broker.com")),
            new Employee(Guid.NewGuid(), new UserId("auth0|6255a0d5c0f77100691fba55"), new HumanName("Sandy", "Cheeks"), new MailAddress("sandy@broker.com")),
            new Employee(Guid.NewGuid(), new UserId("auth0|6255a0d5c0f77100691fba56"), new HumanName("Squid", "Ward"), new MailAddress("squid@broker.com")),
        };
        var insurersA = new List<Employee> {
            new Employee(Guid.NewGuid(), new UserId("auth0|62818ca9b0997000699020da"), new HumanName("Mike", "TheInsurer"), new MailAddress("mike@insurer.com")),
            new Employee(Guid.NewGuid(), new UserId("auth0|62c9565049c4f87c02fce444"), new HumanName("MikeProd", "TheInsurer"), new MailAddress("mike@insurer.com")),
            new Employee(Guid.NewGuid(), new UserId("auth0|62c95739d3dc4f88c18c0b18"), new HumanName("Konrad", "Rotthege"), new MailAddress("konrad@broker.com")),
            new Employee(Guid.NewGuid(), new UserId("auth0|62c956f07b12303583eef4ab"), new HumanName("Jamie", "Brown"), new MailAddress("jamie@broker.com")),
            
        };
        var insurersB = new List<Employee> {
            new Employee(Guid.NewGuid(), new UserId("auth0|bbbbbbbbbbbbbbbbbbbbbbbb"), new HumanName("Mike", "TheInsurer"), new MailAddress("mike@insurer.com")),
        };
        var insurersC = new List<Employee> {
            new Employee(Guid.NewGuid(), new UserId("auth0|cccccccccccccccccccccccc"), new HumanName("Mike", "TheInsurer"), new MailAddress("mike@insurer.com")),
        };

        var tcs = new CompanyFile(Guid.NewGuid(), "tcs.docx", "abc.def", FileType.InsurerTCs, DateTimeOffset.Now);

        var brokerCompanyId = Guid.Parse("c1955284-c461-4a39-9a06-a31b18b23a33");
        var insurerCompanyId = Guid.Parse("b065f679-d19f-4c3f-a948-a29377e91982");
        Data = new List<Company>() { 
            new Company(brokerCompanyId, "BigBrokerCorp", CompanyType.Broker, brokers.ToImmutable(), new EmptyFile()),
            new Company(insurerCompanyId, "BigInsurerCorp A", CompanyType.Insurer, insurersA.ToImmutable(), tcs),
            new Company(Guid.NewGuid(), "BigInsurerCorp B", CompanyType.Insurer, insurersB.ToImmutable(), tcs),
            new Company(Guid.NewGuid(), "BigInsurerCorp C", CompanyType.Insurer, insurersC.ToImmutable(), tcs),
        };
    }
}
