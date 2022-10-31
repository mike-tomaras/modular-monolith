using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.ValueTypes;
using Optional;
using System.Collections.Immutable;

namespace Incepted.Domain.Deals.Application;

public interface IDealService
{
    Option<IImmutableList<DealListItemDTO>, ErrorCode> GetSubmissionsQuery(UserId userId);
    Option<DealSubmissionDTO, ErrorCode> GetSubmissionDetailsQuery(UserId userId, Guid submissionId);
    Option<SubmissionFeedbackDTO, ErrorCode> GetSubmissionFeedbackDetailsQuery(UserId userId, Guid submissionId);
    Option<IImmutableList<SubmissionFeedbackDTO>, ErrorCode> GetAllFeedbackDetailsOfSubmissionQuery(UserId userId, Guid submissionId);
    Option<Guid, ErrorCode> CreateSubmissionCommand(UserId userId, string name);
    Option<DealSubmissionDTO, ErrorCode> UpdateSubmissionDetailsCommand(UserId userId, DealSubmissionDTO submission);
    Option<SubmissionFeedbackDTO, ErrorCode> UpdateSubmissionFeedbackCommand(UserId userId, SubmissionFeedbackDTO submission);
    Option<Unit, ErrorCode> UpdateDealAssigneesCommand(UserId userId, UpdateDealAssigneesDTO assignees);
    Option<Unit, ErrorCode> SubmitSumbissionCommand(UserId userId, SubmitDealDTO submitData);
    Option<Unit, ErrorCode> SubmitFeedbackCommand(UserId userId, SubmitDealFeedbackDTO submitData);
    Option<Unit, ErrorCode> DeclineSubmissionCommand(UserId userId, SubmitDealFeedbackDTO submitData);
    Option<Unit, ErrorCode> ModifySubmissionCommand(UserId userId, ModifySubmittedDealDTO modifyData);
    Option<Unit, ErrorCode> GoLiveCommand(UserId userId, GoLiveDTO goliveData);
    Option<Unit, ErrorCode> NudgeInsurerForFeedback(UserId userId, NudgeDTO nudgeDTO);
    Option<IImmutableList<FileUploadResult>, ErrorCode> UploadSubmissionFilesCommand(UserId userId, Guid submissionId, IEnumerable<(FileDTO metadata, Stream content)> files);
    Option<Unit, ErrorCode> DeleteSubmissionFileCommand(UserId userId, Guid submissionId, Guid fileId);
    Option<FileDownloadResult, ErrorCode> DownloadSubmissionFileCommand(UserId userId, Guid submissionId, Guid fileId);
    Option<Unit, ErrorCode> AcceptFileCommand(UserId userId, AcceptFileDTO acceptData);
    
}
