using Incepted.Domain.Companies.Entities;
using Incepted.Domain.Deals.Application;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using Optional;
using System.Collections.Immutable;

namespace Incepted.Domain.Companies.Application;

internal class CompanyService : ICompanyService
{
    private readonly ICompanyRepo _repo;
    private readonly ICompanyFileService _fileService;
    private readonly ICompanyNotificationService _notificationService;

    public CompanyService(ICompanyRepo repo, ICompanyNotificationService notificationService, ICompanyFileService fileService)
    {
        _repo = repo;
        _notificationService = notificationService;
        _fileService = fileService;
    }

    public Option<IImmutableList<CompanyDTO>, ErrorCode> GetCompaniesOfType(CompanyType type)
    {
        return _repo.GetCompaniesOfType(type).Result
            .Map(companies => companies.Select(Company.Factory.ToDTO).ToImmutable());
    }

    public Option<Company, ErrorCode> GetCompanyOfUserQuery(UserId userId)
    {
        return _repo.GetCompanyOfEmployee(userId).Result;
    }

    public Option<IImmutableList<EmployeeDTO>, ErrorCode> GetEmployeesForCompanyOfUserQuery(UserId userId)
    {
        return _repo
            .GetCompanyOfEmployee(userId).Result
            .Map(company => company.Employees.Select(Employee.Factory.ToDTO).ToImmutable());
    }

    public Option<IImmutableList<EmployeeDTO>, ErrorCode> GetEmployeesForCompanyQuery(Guid companyId)
    {
        return _repo
            .GetCompany(companyId).Result
            .Map(company => company.Employees.Select(Employee.Factory.ToDTO).ToImmutable());
    }

    public Option<Unit, ErrorCode> UpdateEmployeeOfCompanyQuery(UserId userId, UserDTO userDTO)
    {
        Company company = new EmptyCompany();

        return _repo
            .GetCompanyOfEmployee(userId).Result//verify the user is part of a company
            .Map(c => company = c)
            .Map(company => company.Employees.Single(e => e.UserId == userId))
            .FlatMap(employee => employee.Update(userDTO))
            .FlatMap(updatedEmployee => _repo.Update(updatedEmployee, company.Id).Result)
            .FlatMap(brokers => _notificationService.NotifyAdminOfUserDetailsChangeRequest(userId, userDTO));
    }

    public Option<FileDTO, ErrorCode> GetDefaultTsAndCs(UserId userId)
    {
        return _repo
            .GetCompanyOfEmployee(userId).Result//verify the user is part of a company
            .Map(company => CompanyFile.Factory.ToDTO(company.TsAndCs));
    }

    public Option<FileUploadResult, ErrorCode> SetDefaultTsAndCs(UserId userId, FileDTO fileMetadata, Stream fileContent)
    {
        Company company = new EmptyCompany();
        FileUploadResult fileUploadResult = new FileUploadResult(fileMetadata, false, EmptyErrorCode.Empty);

        return _repo
            .GetCompanyOfEmployee(userId).Result//verify the user is part of a company
            .Map(c => company = c)
            .Map(company => _fileService.UploadAsync(company.Id, fileMetadata, fileContent))
            .FlatMap(uploadResultTask => uploadResultTask.Result.SomeWhen(r => r.Uploaded, uploadResultTask.Result.ErrorCode))
            .Map(uploadResult => fileUploadResult = uploadResult)
            .Map(uploadResult => CompanyFile.Factory.ToEntity(uploadResult.File))
            .FlatMap(file => company.SetTcAndCs(file))
            .FlatMap(updatedCompany => _repo.Update(updatedCompany).Result)
            .Map(_ => fileUploadResult);
    }

    public Option<FileDownloadResult, ErrorCode> DownloadTsAndCsCommand(UserId userId)
    {
        Company company = new EmptyCompany();
        CompanyFile tcs = new EmptyFile();
        
        return _repo
            .GetCompanyOfEmployee(userId).Result//verify the user is part of a company
            .Map(c => company = c)
            .FlatMap(company => company.TsAndCs.SomeWhen(file => file != null && file is not EmptyFile), CompanyErrorCodes.DefaultTCsNotFound)
            .Map(file => tcs = file)
            .Map(async file => await _fileService.DownloadAsync(company.Id, file.StoredFileName, file.Type))
            .FlatMap(task => task.Result)
            .Map(content => new FileDownloadResult(CompanyFile.Factory.ToDTO(tcs), content));
    }
}
