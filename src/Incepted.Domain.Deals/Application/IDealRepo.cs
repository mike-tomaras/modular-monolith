using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Optional;
using System.Collections.Immutable;

namespace Incepted.Domain.Deals.Application;

public interface IDealRepo
{
    Task<Option<IImmutableList<DealListItemDTO>, ErrorCode>> GetSubmissions(Guid companyId, CompanyType type);
    Task<Option<DealSubmission, ErrorCode>> GetSubmissionDetails(Guid submissionId);
    Task<Option<SubmissionFeedback, ErrorCode>> GetFeedbackDetails(Guid insurerCompanyId, Guid submissionId);
    Task<Option<IImmutableList<SubmissionFeedback>, ErrorCode>> GetAllFeedbackDetails(Guid submissionId);
    Task<Option<Unit, ErrorCode>> Create(DealSubmission newSubmission);
    Task<Option<Unit, ErrorCode>> Create(SubmissionFeedback newFeedback, DealSubmission submission);
    Task<Option<Unit, ErrorCode>> Create(LiveDeal newLiveDeal);
    Task<Option<Unit, ErrorCode>> Update(DealSubmission updatedSubmission);
    Task<Option<Unit, ErrorCode>> Update(SubmissionFeedback updatedFeedback);
}
