using Incepted.DocGen;
using Incepted.Domain.Companies.Application;
using Incepted.Domain.Companies.Entities;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.IO.Compression;

namespace Incepted.API.Controllers;

public class DocGenController : BaseController
{
    private IDocGenService _docGenService;
    private readonly ICompanyService _companyService;
    private readonly ICompanyFileService _companyFileService;

    public DocGenController(IHttpContextAccessor httpContextAccessor, IDocGenService docGenService, ICompanyService companyService, ICompanyFileService companyFileService) : base(httpContextAccessor)
    {
        _docGenService = docGenService;
        _companyService = companyService;
        _companyFileService = companyFileService;
    }

    /// <summary>
    /// Generates and returns the NBI files
    /// </summary>
    /// <returns>A zip with the generated files</returns>
    /// <response code="200">The zip file was successfully generated</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>    
    [HttpPost("nbi")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Broker,Insurer")]
    public async Task<ActionResult> DownloadNBI([FromBody] SubmissionFeedbackDTO feedbackDTO)
    {
        var company = _companyService.GetCompanyOfUserQuery(UserId).ValueOr(new EmptyCompany());

        if (company is EmptyCompany)
            return EnrichedError(CompanyErrorCodes.CompanyNotFound);

        Log.Information("Generating NBI summary for deal with Id {DealId} and insurer with Id {InsurerId} by user with Id {UserId}",
            feedbackDTO.SubmissionId, company.Id, UserId);        

        var zipFileMemoryStream = new MemoryStream();
        using (ZipArchive archive = new ZipArchive(zipFileMemoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            //NBI docx 
            var tcsFileName = company.HasTsAndCs ? company.TsAndCs.FileName : string.Empty;
            var docStream = _docGenService.GetNBIDoc(feedbackDTO, company.Name, tcsFileName);
            var docEntry = archive.CreateEntry($"NBI - {feedbackDTO.Name}.docx");
            using (var docEntryStream = docEntry.Open())
                docStream.CopyTo(docEntryStream);

            //Excel with pricing and waranties
            var sheetStream = _docGenService.GetNBISheet(feedbackDTO);
            var sheetEntry = archive.CreateEntry($"Annex A - Pricing and Warranties.xlsx");
            using (var sheetEntryStream = sheetEntry.Open())
                sheetStream.CopyTo(sheetEntryStream);

            //T&Cs if they exist
            if (company.HasTsAndCs)
            {
                var bytes = (await _companyFileService.DownloadAsync(company.Id, company.TsAndCs.StoredFileName, Shared.Enums.FileType.InsurerTCs))
                    .ValueOr(new byte[0]);//just unwrap the optional
                var tcsEntry = archive.CreateEntry($"Annex B - T&Cs - {tcsFileName}");
                using (var tcsEntryStream = tcsEntry.Open())
                    tcsEntryStream.Write(bytes, 0, bytes.Length);
            }            
        }

        zipFileMemoryStream.Seek(0, SeekOrigin.Begin);
        return File(zipFileMemoryStream, "application/zip", $"NBI - {feedbackDTO.Name}.zip");
    }

    /// <summary>
    /// Generates and returns the comparison file of all NBIs
    /// </summary>
    /// <returns>The generated file</returns>
    /// <response code="200">The file was successfully generated</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>    
    [HttpPost("nbi/all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Broker")]
    public ActionResult DownloadNBIComparison([FromBody] IEnumerable<SubmissionFeedbackDTO> feedbackDTOs)
    {
        var company = _companyService.GetCompanyOfUserQuery(UserId).ValueOr(new EmptyCompany());

        if (company is EmptyCompany)
            return EnrichedError(CompanyErrorCodes.CompanyNotFound);

        Log.Information("Generating NBI comparison for deal with Id {DealId} by user with Id {UserId}",
            feedbackDTOs.First().SubmissionId, UserId);

        var excelStream = _docGenService.GetNBIComparisonSheet(feedbackDTOs.ToList());
        return File(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"NBI comparison - {feedbackDTOs.First().Name}.xlsx");
    }
}
