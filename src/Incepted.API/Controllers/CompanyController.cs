using Incepted.Domain.Companies.Application;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Incepted.API.Controllers;

public class CompanyController : BaseController
{
    private readonly ICompanyService _companiesService;

    public CompanyController(ICompanyService companiesService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _companiesService = companiesService;
    }

    /// <summary>
    /// Gets all the companies of a certain type
    /// </summary>
    /// <returns>The list of companies of a certain type</returns>
    /// <param name="type">The type of company</param>
    /// <response code="200">The list of companies is returned successfully</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The company was not found</response>
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker")]
    public ActionResult<IEnumerable<CompanyDTO>> GetCompaniesByType([FromQuery] CompanyType type)
    {
        var deal = _companiesService.GetCompaniesOfType(type);

        ActionResult response = Ok();
        deal.Match(
            some: companies => {
                Log.Information("Getting all companies of type {CompanyType}", type.ToString());
                response = Ok(companies);
            },
            none: errorCode => {
                Log.Information("Error while retrieving companies of type {CompanyType}. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}", 
                    type.ToString(), errorCode.status, errorCode.title, errorCode.errors.name.First());
                response = EnrichedError(errorCode);
            }
        );

        return response;
    }

    /// <summary>
    /// Gets the company employees for the company of the logged in user
    /// </summary>
    /// <returns>The company emlpoyees as AssigneeDTO</returns>
    /// <response code="200">The company employees are returned successfully</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The company was not found</response>
    [HttpGet("employees")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker,Insurer")]
    public ActionResult<IEnumerable<EmployeeDTO>> GetEmployees()
    {
        var deal = _companiesService.GetEmployeesForCompanyOfUserQuery(UserId);
        
        ActionResult response = Ok();
        deal.Match(
            some: deal => {
                Log.Information("Received company employee details for the company of user {UserId}", UserId);
                response = Ok(deal);
                },
            none: errorCode => {
                Log.Information("No company found for the user {UserId}. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}", 
                    UserId, errorCode.status, errorCode.title, errorCode.errors.name.First());
                response = EnrichedError(errorCode);
            }
        );

        return response;
    }

    /// <summary>
    /// Updates the details of a company employee
    /// </summary>
    /// <returns>Nothing</returns>
    /// <response code="200">The company employee details were updated successfully</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The employee was not found</response>
    [HttpPut("employee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker,Insurer")]
    public ActionResult UpdateEmployee(UserDTO updateDTO)
    {
        var deal = _companiesService.UpdateEmployeeOfCompanyQuery(UserId, updateDTO);

        ActionResult response = Ok();
        deal.Match(
            some: deal => {
                Log.Information("Updated company employee details for {UserId}", UserId);
                response = Ok(deal);
            },
            none: errorCode => {
                Log.Information("Could not update the details of employee with user ID {UserId}. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                    UserId, errorCode.status, errorCode.title, errorCode.errors.name.First());
                response = EnrichedError(errorCode);
            }
        );

        return response;
    }

    /// <summary>
    /// Gets the insurance company's default TandCs for the company of the logged in user
    /// </summary>
    /// <returns>The insurance company's default TandCs</returns>
    /// <response code="200">The insurance company's default TandCs is returned successfully</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The company was not found</response>
    [HttpGet("tcs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Insurer")]
    public ActionResult<FileDTO> GetTsAndCs()
    {
        var file = _companiesService.GetDefaultTsAndCs(UserId);

        ActionResult response = Ok();
        file.Match(
            some: file => {
                Log.Information("Received insurance company's default T&Cs for the company of user {UserId}", UserId);
                response = Ok(file);
            },
            none: errorCode => {
                Log.Information("No default T&Cs found for the company of the user {UserId}. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                    UserId, errorCode.status, errorCode.title, errorCode.errors.name.First());
                response = EnrichedError(errorCode);
            }
        );

        return response;
    }

    /// <summary>
    /// Sets the insurance company's default TandCs for the company of the logged in user
    /// </summary>
    /// <returns>Nothing</returns>
    /// <response code="200">The insurance company's default TandCs is saved successfully</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The company was not found</response>
    [HttpPost("tcs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Insurer")]
    public ActionResult<IEnumerable<EmployeeDTO>> SetTsAndCs([FromForm] IFormFile files)
    {
        //"files" is only one here, have to name it in plural to reuse the code in the SPA DealFileService:118
        var validationUploadResults = new List<FileUploadResult>();
        if (!FileValidations.ValidateFile(files, FileType.InsurerTCs, validationUploadResults))        
            return EnrichedError(validationUploadResults.First().ErrorCode);

        var timestamp = DateTimeOffset.UtcNow;
        var fileMetadata = new FileDTO(Id: Guid.NewGuid(),
                                    FileName: files.FileName,
                                    StoredFileName: $"{FileType.InsurerTCs}-{timestamp.Ticks}-{Path.GetRandomFileName()}",
                                    Type: FileType.InsurerTCs,
                                    LastModified: timestamp,
                                    ContentType: files.ContentType);                  

        var deal = _companiesService.SetDefaultTsAndCs(UserId, fileMetadata, files.OpenReadStream());

        ActionResult response = Ok();
        deal.Match(
            some: deal => {
                Log.Information("Uploaded insurance company's default T&Cs for the company of user {UserId}", UserId);
                response = Ok(deal);
            },
            none: errorCode => {
                Log.Information("No default T&Cs found for the company of the user {UserId}. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                    UserId, errorCode.status, errorCode.title, errorCode.errors.name.First());
                response = EnrichedError(errorCode);
            }
        );

        return response;
    }

    /// <summary>
    /// Downloads the insurance company's default TandCs
    /// </summary>
    /// <returns>The file contens</returns>
    /// <response code="200">The file was found and is downloading</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpGet("tcs/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Insurer")]
    public ActionResult DownloadTcs()
    {
        ActionResult response = Ok();

        _companiesService.DownloadTsAndCsCommand(UserId)
            .Match(
                some: downloadResult =>
                {
                    Log.Information("Successfully downloading insurance company's default TandCs for company of user with Id {UserId}", UserId);

                    response = File(
                        downloadResult.Content,
                        downloadResult.File.ContentType,
                        downloadResult.File.FileName);

                },
                none: errorCode =>
                {
                    Log.Error("Downloading insurance company's default TandCs could not be completed. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }
}
