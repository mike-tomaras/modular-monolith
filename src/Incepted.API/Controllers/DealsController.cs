using Incepted.Domain.Deals.Application;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Incepted.API.Controllers;

public class DealsController : BaseController
{
    private readonly IDealService _dealsService;

    public DealsController(IDealService dealsService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
        _dealsService = dealsService;
    }

    /// <summary>
    /// Gets a list of deals for the specified status and for the logged in user only
    /// </summary>
    /// <returns>The list of deals (no pagination)</returns>
    /// <response code="200">The list of deals is returned successfully</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="402">The user is not authenticated</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Broker,Insurer")]
    public ActionResult<IEnumerable<DealListItemDTO>> GetAllSubmissions()
    {
        ActionResult response = Ok();

        _dealsService.GetSubmissionsQuery(UserId)
            .Match(
                some: deals =>
                {
                    Log.Information("Received deal submission list for user {UserId}", UserId);
                    response = Ok(deals.Select(d => (object)d)); //must cast to object to allow the serializer to serialize extra props of the child classes, see https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-polymorphism
                },
                none: errorCode =>
                {
                    Log.Error("No deal submissions found for user {UserId}. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        UserId, errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Gets the deal details for the specified deal ID
    /// </summary>
    /// <param name="submissionId">The ID of the deal</param>
    /// <returns>The deal details</returns>
    /// <response code="200">The deal details are returned successfully</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpGet("{submissionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker,Insurer")]
    public ActionResult<DealSubmissionDTO> GetDealSubmission(Guid submissionId)
    {
        ActionResult response = Ok();

        _dealsService.GetSubmissionDetailsQuery(UserId, submissionId)
            .Match(
                some: deal =>
                {
                    Log.Information("Received deal details for id {DealId} and user {UserId}", submissionId, UserId);
                    response = Ok(deal);
                },
                none: errorCode =>
                {
                    Log.Error("No deal details found for id {DealId} and user {UserId}. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        submissionId, UserId, errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Gets the deal feedback for the specified deal ID
    /// </summary>
    /// <param name="submissionId">The ID of the deal (the feedback id will be inferred from the logged in user's insurance company)</param>
    /// <returns>The deal feedback details</returns>
    /// <response code="200">The deal feedback details are returned successfully</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpGet("{submissionId}/feedback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Insurer")]
    public ActionResult<DealSubmissionDTO> GetSingleSubmissionFeedback(Guid submissionId)
    {
        ActionResult response = Ok();

        _dealsService.GetSubmissionFeedbackDetailsQuery(UserId, submissionId)
            .Match(
                some: deal =>
                {
                    Log.Information("Received deal feedback details for id {DealId} and user {UserId}", submissionId, UserId);
                    response = Ok(deal);
                },
                none: errorCode =>
                {
                    Log.Error("No deal feedback details found for id {DealId} and user {UserId}. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        submissionId, UserId, errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Gets all the deal feedback for the specified deal ID
    /// </summary>
    /// <param name="submissionId">The ID of the deal</param>
    /// <returns>All the deal feedback details</returns>
    /// <response code="200">All the deal feedback details are returned successfully</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpGet("{submissionId}/feedback/all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker")]
    public ActionResult<IEnumerable<SubmissionFeedbackDTO>> GetSingleSubmissionsFeedbackFromAllInsurers(Guid submissionId)
    {
        ActionResult response = Ok();

        _dealsService.GetAllFeedbackDetailsOfSubmissionQuery(UserId, submissionId)
            .Match(
                some: deal =>
                {
                    Log.Information("Received all deal feedback details for id {DealId} and user {UserId}", submissionId, UserId);
                    response = Ok(deal);
                },
                none: errorCode =>
                {
                    Log.Error("No deal feedback summary details found for id {DealId} and user {UserId}. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        submissionId, UserId, errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Creates a deal with the specified name and no other data
    /// </summary>
    /// <param name="createDealDTO">The name of the new deal</param>
    /// <returns>The deal details</returns>
    /// <response code="201">The new deal was created</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Broker")]
    public ActionResult CreateSubmission([FromBody] CreateDealDTO createDealDTO)
    {
        ActionResult response = Ok();

        _dealsService.CreateSubmissionCommand(UserId, createDealDTO.Name)
            .Match(
                some: dealId =>
                {
                    Log.Information("Created deal with name {DealName} and Id {DealId}", createDealDTO.Name, dealId);
                    string newDealUrl = $"api/v1/deals/{dealId}";
                    response = Created(newDealUrl, dealId);
                },
                none: errorCode =>
                {
                    Log.Error("Deal could not be created. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Updates an existing deal except the assignees, use PUT /deals/assignees for that
    /// </summary>
    /// <param name="submission">The the deal to update (replace in the db)</param>
    /// <returns>Nothing in the reponse content</returns>
    /// <response code="200">The deal was updated</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker")]
    public ActionResult UpdateSubmission(DealSubmissionDTO submission)
    {
        ActionResult response = Ok();

        _dealsService.UpdateSubmissionDetailsCommand(UserId, submission)
            .Match(
                some: updatedDeal =>
                {
                    Log.Information("Updated deal with name {DealName} and Id {DealId}", submission.Name, submission.Id);
                    response = Ok(updatedDeal);
                },
                none: errorCode =>
                {
                    Log.Error("Deal could not be updated. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Updates an existing deal feedback
    /// </summary>
    /// <param name="feedback">The the deal to update (replace in the db)</param>
    /// <returns>Nothing in the reponse content</returns>
    /// <response code="200">The deal was updated</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpPut("feedback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Insurer")]
    public ActionResult UpdateFeedback(SubmissionFeedbackDTO feedback)
    {
        ActionResult response = Ok();

        _dealsService.UpdateSubmissionFeedbackCommand(UserId, feedback)
            .Match(
                some: updatedDeal =>
                {
                    Log.Information("Updated deal feedback with name {DealName} and Id {DealId}", feedback.Name, feedback.Id);
                    response = Ok(updatedDeal);
                },
                none: errorCode =>
                {
                    Log.Error("Deal feedback could not be updated. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }


    /// <summary>
    /// Updates an existing deal's assignees only
    /// </summary>
    /// <param name="assignees">The the deal assignees to update (replace in the db)</param>
    /// <returns>Nothing in the reponse content</returns>
    /// <response code="200">The deal was updated</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpPut("assignees")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker,Insurer")]
    public ActionResult UpdateAssignees([FromBody] UpdateDealAssigneesDTO assignees)
    {
        ActionResult response = Ok();

        _dealsService.UpdateDealAssigneesCommand(UserId, assignees)
            .Match(
                some: _ =>
                {
                    Log.Information("Updated deal assignees for deal with Id {DealId}", assignees.DealId);
                    response = Ok();
                },
                none: errorCode =>
                {
                    Log.Error("Deal assignees could not be updated. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Submits a deal to insurers for review and feedback
    /// </summary>
    /// <param name="submitData">The the deal id and the insurer info for submission</param>
    /// <returns>Nothing in the reponse content</returns>
    /// <response code="200">The deal was submitted</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpPut("submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker")]
    public ActionResult SubmitDeal([FromBody] SubmitDealDTO submitData)
    {
        ActionResult response = Ok();

        _dealsService.SubmitSumbissionCommand(UserId, submitData)
            .Match(
                some: _ =>
                {
                    Log.Information("Submitted deal with Id {DealId} for insurer feedback", submitData.DealId);
                    response = Ok();
                },
                none: errorCode =>
                {
                    Log.Error("Deal submission could not finish. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Modifies a deal after it's been submitted to insurers for review and feedback
    /// </summary>
    /// <param name="modifyData">The the deal id and the modification information to be sent to insurers</param>
    /// <returns>Nothing in the reponse content</returns>
    /// <response code="200">The deal was modified</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpPut("modifysubmission")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker")]
    public ActionResult ModifySubmittedDeal([FromBody] ModifySubmittedDealDTO modifyData)
    {
        ActionResult response = Ok();

        _dealsService.ModifySubmissionCommand(UserId, modifyData)
            .Match(
                some: _ =>
                {
                    Log.Information("Modified submitted deal with Id {DealId} for insurer feedback", modifyData.DealId);
                    response = Ok();
                },
                none: errorCode =>
                {
                    Log.Error("Deal submission could not be modified. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Sets a deal to live with a selected insurer and notifies the rest of the declined insurers
    /// </summary>
    /// <param name="goliveData">The the deal, feedback and insurance company ids to go live with</param>
    /// <returns>Nothing in the reponse content</returns>
    /// <response code="200">The deal was set to live</response>
    /// <response code="400">The request was malformed or the deal/feedback are not in a correct status to go live with</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpPut("golive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker")]
    public ActionResult GoLiveForDeal([FromBody] GoLiveDTO goliveData)
    {
        ActionResult response = Ok();

        _dealsService.GoLiveCommand(UserId, goliveData)
            .Match(
                some: _ =>
                {
                    Log.Information("Set deal with Id {DealId} live for insurer feedback {FeedbackId}", goliveData.SubmissionId, goliveData.FeedbackId);
                    response = Ok();
                },
                none: errorCode =>
                {
                    Log.Error("Deal could not be set to live. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Submits an insurer's feedback, otherwise known as an NBI and then notifies the broker
    /// </summary>
    /// <param name="submitData">The the deal and feedback ids to mark as submitted</param>
    /// <returns>Nothing in the reponse content</returns>
    /// <response code="200">The feedback was submitted</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpPut("feedback/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Insurer")]
    public ActionResult SubmitDealDeedback([FromBody] SubmitDealFeedbackDTO submitData)
    {
        ActionResult response = Ok();

        _dealsService.SubmitFeedbackCommand(UserId, submitData)
            .Match(
                some: _ =>
                {
                    Log.Information("Submitted feedback for deal with Id {DealId}", submitData.SubmissionId);
                    response = Ok();
                },
                none: errorCode =>
                {
                    Log.Error("Submission feedback could not be performed. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Declines a submission for an insurer and then notifies the broker
    /// </summary>
    /// <param name="declineData">The the deal and feedback ids to mark as declined</param>
    /// <returns>Nothing in the reponse content</returns>
    /// <response code="200">The feedback was declined</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpPut("feedback/decline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Insurer")]
    public ActionResult DeclineDealDeedback([FromBody] SubmitDealFeedbackDTO declineData)
    {
        ActionResult response = Ok();

        _dealsService.DeclineSubmissionCommand(UserId, declineData)
            .Match(
                some: _ =>
                {
                    Log.Information("Declined deal with Id {DealId}", declineData.SubmissionId);
                    response = Ok();
                },
                none: errorCode =>
                {
                    Log.Error("Submission decline could not be performed. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Sends a reminder to an insurer to give feedbac for a submission. 
    /// </summary>
    /// <param name="nudgeDTO">The deal id, feedback id to send a reminder about and insurance company id to send e reminder to</param>
    /// <returns>Nothing in the reponse content</returns>
    /// <response code="200">The reminder was sent</response>
    /// <response code="400">The request was malformed or the submission is not in a correct status</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpPut("feedback/nudge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker")]
    public ActionResult NudgeInsurerForFeedback([FromBody] NudgeDTO nudgeDTO)
    {
        ActionResult response = Ok();

        _dealsService.NudgeInsurerForFeedback(UserId, nudgeDTO)
            .Match(
                some: _ =>
                {
                    Log.Information("Sent reminder to Insurer with Id {InsuranceCompanyId} for deal with Id {DealId}", nudgeDTO.InsuranceCompanyId, nudgeDTO.SubmissionId);
                    response = Ok();
                },
                none: errorCode =>
                {
                    Log.Error("Submission reminder could not be sent. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Accepts a file on behalf of an insurer
    /// </summary>
    /// <param name="acceptData">The the file to be accepted</param>
    /// <returns>Nothing in the reponse content</returns>
    /// <response code="200">The file was accepted</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpPut("acceptfile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Insurer")]
    public ActionResult AcceptFile([FromBody] AcceptFileDTO acceptData)
    {
        ActionResult response = Ok();

        _dealsService.AcceptFileCommand(UserId, acceptData)
            .Match(
                some: _ =>
                {
                    Log.Information("Accepted deal {FileType} file with Deal Id {DealId} and File Id {FileId}", 
                        acceptData.File.Type.ToString(), acceptData.SubmissionId, acceptData.File.Id);
                    response = Ok();
                },
                none: errorCode =>
                {
                    Log.Error("Could not accept deal {FileType} file for Deal Id {DealId}. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        acceptData.File.Type.ToString(), acceptData.SubmissionId, errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Uploads files to a deal
    /// </summary>
    /// <param name="dealId">The ID of the deal</param>
    /// <param name="type">The type of the file(s) being uploaded</param>
    /// <param name="files">The files to upload</param>
    /// <returns>The upload status of each file in a list</returns>
    /// <response code="201">The files were uploaded</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpPost("{dealId}/file/{type}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker")]
    public ActionResult<IEnumerable<FileUploadResult>> UploadFiles(Guid dealId, FileType type, [FromForm] IEnumerable<IFormFile> files)
    {
        ActionResult response = Ok();
        var validationUploadResults = new List<FileUploadResult>();

        var filesToUpload = new List<(FileDTO metadata, Stream content)>();
        foreach (var file in files)
        {
            //if validation fails, the upload results will have the negative result in them
            //otherwise add the file to the list for files to upload
            if (FileValidations.ValidateFile(file, type, validationUploadResults))
            {
                var timestamp = DateTimeOffset.UtcNow;
                filesToUpload.Add((metadata: new FileDTO(Id: Guid.NewGuid(),
                                        FileName: file.FileName,
                                        StoredFileName: $"{type}-{timestamp.Ticks}-{Path.GetRandomFileName()}",
                                        Type: type,
                                        LastModified: timestamp,
                                        ContentType: file.ContentType),
                      content: file.OpenReadStream()));
            }
        }

        _dealsService.UploadSubmissionFilesCommand(UserId, dealId, filesToUpload)
            .Match(
                some: uploadResults =>
                {
                    Log.Information("Finished processing deal files for deal with Id {DealId}", dealId);
                    response = Ok(uploadResults.Concat(validationUploadResults));
                },
                none: errorCode =>
                {
                    Log.Error("Processing file uploads could not be completed. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Deletes a file from a deal
    /// </summary>
    /// <param name="dealId">The ID of the deal</param>
    /// <param name="fileId">The ID of the file to delete</param>
    /// <returns>Nothing in the reponse content</returns>
    /// <response code="200">The file was deleted</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpDelete("{dealId}/file/{fileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker")]
    public ActionResult DeleteFile(Guid dealId, Guid fileId)
    {
        ActionResult response = Ok();

        _dealsService.DeleteSubmissionFileCommand(UserId, dealId, fileId)
            .Match(
                some: uploadResults =>
                {
                    Log.Information("Finished deleting deal file with Id {FileId} for deal with Id {DealId}", fileId, dealId);
                    response = Ok(uploadResults);
                },
                none: errorCode =>
                {
                    Log.Error("Deleting file could not be completed. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }

    /// <summary>
    /// Downloads a file from a deal
    /// </summary>
    /// <param name="dealId">The ID of the deal</param>
    /// <param name="fileId">The ID of the file to download</param>
    /// <returns>The file contene</returns>
    /// <response code="200">The file was found and is downloading</response>
    /// <response code="400">The request was malformed</response>
    /// <response code="401">The authenticated user does not have permission to call this endpoint</response>
    /// <response code="403">The user is not authenticated</response>
    /// <response code="404">The deal was not found</response>
    [HttpGet("{dealId}/file/{fileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Broker")]
    public ActionResult DownloadFile(Guid dealId, Guid fileId)
    {
        ActionResult response = Ok();

        _dealsService.DownloadSubmissionFileCommand(UserId, dealId, fileId)
            .Match(
                some: downloadResult =>
                {
                    Log.Information("Successfully downloading deal file with Id {FileId} for deal with Id {DealId}", fileId, dealId);

                    response = File(
                        downloadResult.Content,
                        downloadResult.File.ContentType,
                        downloadResult.File.FileName);

                },
                none: errorCode =>
                {
                    Log.Error("Downloading file could not be completed. Reason: {ErrorStatus} | {ErrorMessage} | {ErrorDetails}",
                        errorCode.status, errorCode.title, errorCode.errors.name.First());
                    response = EnrichedError(errorCode);
                }
        );

        return response;
    }
}
