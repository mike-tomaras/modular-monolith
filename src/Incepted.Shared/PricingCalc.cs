using Incepted.Shared.ValueTypes;

namespace Incepted.Shared;

public static class PricingCalc
{
    public static decimal RoL(Money ev, Money premium, Limit limit)
    {
        if (ev.Amount == 0 || limit.Value == 0) return 0;

        return premium.Amount / limit.ToAmount(ev) * 100;
    }

    public static string RoLString(Money ev, Money premium, Limit limit) => 
        $"{RoL(ev, premium, limit):0.##}%";

    public static decimal EnhancementValue(Money premium, IEnumerable<Enhancement> enhancements) =>
        enhancements.Aggregate(0m, (currentSum, nextEnhancement) => currentSum += premium.Amount * (decimal)nextEnhancement.AP);

    public static string EnhancementValueString(Money premium, IEnumerable<Enhancement> enhancements)
    {
        var numericValue = EnhancementValue(premium, enhancements);

        return numericValue == 0 
            ? "n/a"
            : numericValue.ToString("0.##");
    }        

    public static decimal Total(Money premium, IEnumerable<Enhancement> enhancements, Money uwFee) =>
        premium.Amount + EnhancementValue(premium, enhancements) + uwFee.Amount;

    public static string TotalString(Money premium, IEnumerable<Enhancement> enhancements, Money uwFee)
    {
        var numericValue = Total(premium, enhancements, uwFee);

        return numericValue == 0
            ? "N/A"
            : numericValue.ToString("0.##");
    }

}
