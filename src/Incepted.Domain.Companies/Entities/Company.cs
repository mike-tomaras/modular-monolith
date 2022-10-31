using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using Optional;
using System.Collections.Immutable;

namespace Incepted.Domain.Companies.Entities;

public class Company
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public CompanyType Type { get; private set; }
    public IImmutableList<Employee> Employees { get; private set; }
    public CompanyFile TsAndCs { get; private set; }

    public bool HasTsAndCs => TsAndCs != null && TsAndCs is not EmptyFile;

    public Company(Guid id, string name, CompanyType type, IImmutableList<Employee> employees, CompanyFile tsAndCs)
    {
        if (id == Guid.Empty) throw new ArgumentException("Company Id can't be empty", $"{nameof(Company)} {nameof(id)}");
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Company name can't be empty", $"{nameof(Company)} {nameof(name)}");
        if (employees == null || !employees.Any()) throw new ArgumentException("Company must have at least one employee", $"{nameof(Company)} {nameof(employees)}");

        Id = id;
        Name = name;
        Type = type;
        Employees = employees;
        TsAndCs = tsAndCs;
    }

    internal Option<Company, ErrorCode> SetTcAndCs(CompanyFile file) => 
        new Company(Id, Name, Type, Employees, file).Some<Company, ErrorCode>();

    public bool AreAssigneesValidEmployees(IEnumerable<UserId> assigneeIds)
    {
        return assigneeIds.All(id => Employees.Any(e => e.UserId == id));
    }

    public static class Factory
    {
        public static CompanyDTO ToDTO(Company company)
        {
            return new CompanyDTO(company.Id, company.Name);
        }
    }
}

public class EmptyCompany : Company
{
    public EmptyCompany() :base(Guid.NewGuid(), "empty", CompanyType.Broker, ImmutableList.Create<Employee>(new EmptyEmployee()), new EmptyFile())
    { }
}
