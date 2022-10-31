using Incepted.Shared.DTOs;

namespace Incepted.DocGen;

public interface IDocGenService
{
    Stream GetNBIDoc(SubmissionFeedbackDTO feedback, string insurerName, string tcsFileName);
    Stream GetNBISheet(SubmissionFeedbackDTO feedback);
    Stream GetNBIComparisonSheet(List<SubmissionFeedbackDTO> feedbacks);
}
