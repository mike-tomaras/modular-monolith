using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;

namespace Incepted.Shared;

public static class CoverageCalc
{
    private const int SCORE_YES = 5;
    private const int SCORE_PARTIAL = 3;
    private const int SCORE_TBC = 1;
    
    public static double CoveragePcnt(IEnumerable<Warranty> warranties)
    {
        var maxScore = warranties.Count() * SCORE_YES;

        var score = warranties.Aggregate(0, (current, next) => 
            current += next.CoveragePosition switch
            {
                CoveragePosition.Yes => SCORE_YES,
                CoveragePosition.Partial => SCORE_PARTIAL,
                CoveragePosition.TBC => SCORE_TBC,
                _ => 0
            }
        );

        return Math.Round((double)(score * 100 / maxScore), 1);
    }
}
