using Incepted.Shared.ValueTypes;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Incepted.Shared.DTOs;

public record class DealListItemDTO(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("broker")] string BrokerName,
    [property: JsonPropertyName("industry")] string Industry,
    [property: JsonPropertyName("spaJur")] string SpaJurisdiction,
    [property: JsonPropertyName("isSubmitted")] bool IsSubmittedToInsurers,
    [property: JsonPropertyName("ev")] Money EnterpriseValue,
    [property: JsonPropertyName("assignees")] IImmutableList<EmployeeDTO> Assignees,
    [property: JsonPropertyName("submissionDeadline")] DateTimeOffset? SubmissionDeadline
    )
{
    [JsonIgnore] public string AssigneesString => Assignees
            .Aggregate("Assignees:", (current, next) => $"{current} {next.FullName} -")
            .TrimEnd('-').Trim();

    [JsonIgnore] public string SubmissionDeadlineString => SubmissionDeadline.HasValue
        ? SubmissionDeadline.Value.ToLocalTime().ToString("d")
        : "Not set";
}


public record class DealSubmissionDTO(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("broker")] string BrokerName,
    [property: JsonPropertyName("terms")] BasicTerms Terms,
    [property: JsonPropertyName("pricing")] SubmissionPricing Pricing,
    [property: JsonPropertyName("enhancements")] IImmutableList<Enhancement> Enhancements,
    [property: JsonPropertyName("warranties")] IImmutableList<Warranty> Warranties,
    [property: JsonPropertyName("assignees")] IImmutableList<EmployeeDTO> Assignees,
    [property: JsonPropertyName("files")] IImmutableList<FileDTO> Files,
    [property: JsonPropertyName("insurers")] IImmutableList<(Guid InsurerId, Guid FeedbackId)> InsurerFeedbacks,
    [property: JsonPropertyName("modifications")] IImmutableList<Modification> Modifications,
    [property: JsonPropertyName("submissionDeadline")] DateTimeOffset? SubmissionDeadline,
    [property: JsonPropertyName("etag")] string ETag
    )
{
    public FileDTO? SPA => Files
                            .Where(f => f.Type == Enums.FileType.SPA)
                            .OrderByDescending(f => f.LastModified)
                            .FirstOrDefault();
    public FileDTO? NDA => Files
                            .Where(f => f.Type == Enums.FileType.NDA)
                            .OrderByDescending(f => f.LastModified)
                            .FirstOrDefault();

    public bool IsSubmittedToInsurers => InsurerFeedbacks.Any();

    public IImmutableList<FileDTO> AddFiles(IEnumerable<FileDTO> newFiles) => 
        Files
            .Concat(newFiles)
            .OrderByDescending(f => f.LastModified)
            .ToImmutable();

    public static class Factory
    {
        public static DealSubmissionDTO Empty =>
            new DealSubmissionDTO(Guid.Empty, string.Empty, string.Empty, new BasicTerms(), new SubmissionPricing(), Enhancement.Factory.Default, ImmutableList.Create<Warranty>(), ImmutableList.Create<EmployeeDTO>(), ImmutableList.Create<FileDTO>(), ImmutableList.Create<(Guid InsurerId, Guid FeedbackId)>(), ImmutableList.Create<Modification>(), DateTimeOffset.UtcNow, string.Empty);
    }
}


public record class CreateDealDTO(
    [property: JsonPropertyName("name")] string Name
);
public record class SubmitDealDTO(
    [property: JsonPropertyName("id")] Guid DealId,
    [property: JsonPropertyName("companies")] IImmutableList<CompanyDTO> InsurersToSubmitTo,
    //[property: JsonPropertyName("emails")] IImmutableList<string> Emails,    
    [property: JsonPropertyName("deadline")] DateTimeOffset SubmissionDeadline
);
public record class SubmitDealFeedbackDTO(
    [property: JsonPropertyName("feedbackId")] Guid FeedbackId,    
    [property: JsonPropertyName("submissionId")] Guid SubmissionId    
);
public record class ModifySubmittedDealDTO(
    [property: JsonPropertyName("id")] Guid DealId,    
    [property: JsonPropertyName("notes")] string Notes
);

public record class GoLiveDTO(
    [property: JsonPropertyName("feedbackId")] Guid FeedbackId,
    [property: JsonPropertyName("submissionId")] Guid SubmissionId,
    [property: JsonPropertyName("insurerId")] Guid InsuranceCompanyId
);

public record class SubmissionFeedbackDTO(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("submissionId")] Guid SubmissionId,
    [property: JsonPropertyName("insuranceCompanyId")] Guid InsuranceCompanyId,
    [property: JsonPropertyName("insuranceCompanyName")] string InsuranceCompanyName,
    [property: JsonPropertyName("ndaAccepted")] bool NdaAccepted,
    [property: JsonPropertyName("submitted")] bool Submitted,
    [property: JsonPropertyName("declined")] bool Declined,
    [property: JsonPropertyName("isLive")] bool IsLive,
    [property: JsonPropertyName("forReview")] bool ForReview,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("notes")] string Notes,
    [property: JsonPropertyName("pricing")] FeedbackPricing Pricing,
    [property: JsonPropertyName("enhancements")] IImmutableList<Enhancement> Enhancements,
    [property: JsonPropertyName("exclusions")] IImmutableList<Exclusion> Exclusions,
    [property: JsonPropertyName("exclusdedCountries")] IImmutableList<string> ExcludedCountries,
    [property: JsonPropertyName("uwfocus")] IImmutableList<string> UwFocus,
    [property: JsonPropertyName("warranties")] IImmutableList<Warranty> Warranties,
    [property: JsonPropertyName("etag")] string ETag
    )
{
    public static class Factory
    {
        public static SubmissionFeedbackDTO Empty =>
            new SubmissionFeedbackDTO(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty, false, false, false, false, false, string.Empty, string.Empty, new FeedbackPricing(), Enhancement.Factory.Default, Exclusion.Factory.Default, ImmutableList.Create<string>(), ImmutableList.Create<string>(), Warranty.Factory.Default, string.Empty);
    }
}

public record class NudgeDTO(
    [property: JsonPropertyName("feedbackId")] Guid FeedbackId,
    [property: JsonPropertyName("submissionId")] Guid SubmissionId,
    [property: JsonPropertyName("insurerId")] Guid InsuranceCompanyId
);

public record class AcceptFileDTO(
    [property: JsonPropertyName("feedbackId")] Guid FeedbackId,
    [property: JsonPropertyName("submissionId")] Guid SubmissionId,
    [property: JsonPropertyName("insurerId")] Guid InsuranceCompanyId,
    [property: JsonPropertyName("file")] FileDTO File    
);
