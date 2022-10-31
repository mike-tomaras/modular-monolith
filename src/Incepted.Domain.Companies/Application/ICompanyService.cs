using Incepted.Domain.Companies.Entities;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using Optional;
using System.Collections.Immutable;

namespace Incepted.Domain.Companies.Application;

public interface ICompanyService
{
    public Option<IImmutableList<CompanyDTO>, ErrorCode> GetCompaniesOfType(CompanyType type);
    public Option<Company, ErrorCode> GetCompanyOfUserQuery(UserId userId);
    public Option<IImmutableList<EmployeeDTO>, ErrorCode> GetEmployeesForCompanyOfUserQuery(UserId userId);
    public Option<IImmutableList<EmployeeDTO>, ErrorCode> GetEmployeesForCompanyQuery(Guid companyId);
    public Option<Unit, ErrorCode> UpdateEmployeeOfCompanyQuery(UserId userId, UserDTO userDTO);
    public Option<FileDTO, ErrorCode> GetDefaultTsAndCs(UserId userId);
    public Option<FileUploadResult, ErrorCode> SetDefaultTsAndCs(UserId userId, FileDTO fileMetadata, Stream fileContent);
    public Option<FileDownloadResult, ErrorCode> DownloadTsAndCsCommand(UserId userId);
}
