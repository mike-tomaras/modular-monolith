using Incepted.Shared.DTOs;

namespace Incepted.Shared;

public static class RankingCalc
{
    public static IDictionary<SubmissionFeedbackDTO, int> Rank(IEnumerable<SubmissionFeedbackDTO> feedbacks)
    {
        var scores = feedbacks.ToDictionary(f => f, f => 0);
        var allResults = LowestPrice(feedbacks).Concat(LowestDeminimis(feedbacks)).Concat(HighestCoverage(feedbacks));
        foreach (var feedback in allResults)
        {
            scores[feedback]++;
        }

        return scores;
    }

    //all ranking functions return IEnumerable to account for ties

    public static IEnumerable<SubmissionFeedbackDTO> LowestPrice(IEnumerable<SubmissionFeedbackDTO> feedbacks)
    {
        if (feedbacks.Count() < 2) return new List<SubmissionFeedbackDTO>();

        var minPrice = decimal.MaxValue;
        var feedbackWithBestPrice = new List<SubmissionFeedbackDTO>();
        foreach (var feedback in feedbacks)
        {
            var feedbackMinPrice = feedback.Pricing.Options
                .Min(option => PricingCalc.Total(option.Premium, feedback.Enhancements, feedback.Pricing.UwFee));

            if (feedbackMinPrice < minPrice)
            {
                feedbackWithBestPrice = new List<SubmissionFeedbackDTO> { feedback };
                minPrice = feedbackMinPrice;
            }                
            else if (feedbackMinPrice == minPrice) feedbackWithBestPrice.Add(feedback);
        }

        return feedbackWithBestPrice;
    }

    public static IEnumerable<SubmissionFeedbackDTO> LowestDeminimis(IEnumerable<SubmissionFeedbackDTO> feedbacks)
    {
        if (feedbacks.Count() < 2) return new List<SubmissionFeedbackDTO>();

        var minDeminimis = decimal.MaxValue;
        var feedbackWithBestDeminimis = new List<SubmissionFeedbackDTO>();
        foreach (var feedback in feedbacks)
        {
            if (feedback.Pricing.DeMinimis.Amount < minDeminimis)
            {
                feedbackWithBestDeminimis = new List<SubmissionFeedbackDTO> { feedback };
                minDeminimis = feedback.Pricing.DeMinimis.Amount;
            }
            else if (feedback.Pricing.DeMinimis.Amount == minDeminimis) feedbackWithBestDeminimis.Add(feedback);
        }
        
        return feedbackWithBestDeminimis;
    }

    public static IEnumerable<SubmissionFeedbackDTO> HighestCoverage(IEnumerable<SubmissionFeedbackDTO> feedbacks)
    {
        if (feedbacks.Count() < 2) return new List<SubmissionFeedbackDTO>();

        var maxCoverage = 0d;
        var feedbackWithBestCoverage = new List<SubmissionFeedbackDTO>();
        foreach (var feedback in feedbacks)
        {
            var coverage = CoverageCalc.CoveragePcnt(feedback.Warranties);
            if (coverage > maxCoverage)
            {
                feedbackWithBestCoverage = new List<SubmissionFeedbackDTO> { feedback };
                maxCoverage = coverage;
            }                
            else if (coverage == maxCoverage) feedbackWithBestCoverage.Add(feedback);
        }

        return feedbackWithBestCoverage;
    }
}
