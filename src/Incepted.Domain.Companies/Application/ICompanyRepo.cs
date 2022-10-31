using Incepted.Domain.Companies.Entities;
using Incepted.Shared;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using Optional;
using System.Collections.Immutable;

namespace Incepted.Domain.Companies.Application;

public interface ICompanyRepo
{
    Task<Option<IImmutableList<Company>, ErrorCode>> GetCompaniesOfType(CompanyType type);
    Task<Option<Company, ErrorCode>> GetCompanyOfEmployee(UserId userId);
    Task<Option<Company, ErrorCode>> GetCompany(Guid id);
    Task<Option<Unit, ErrorCode>> Update(Company company);
    Task<Option<Unit, ErrorCode>> Update(Employee employee, Guid companyId);
}
