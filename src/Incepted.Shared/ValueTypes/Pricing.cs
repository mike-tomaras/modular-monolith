using Incepted.Shared.Enums;
using Optional;
using System.Text.Json.Serialization;

namespace Incepted.Shared.ValueTypes;

public class SubmissionPricing
{
    [JsonPropertyName("ev")] public Money EnterpriseValue { get; private init; }
    [JsonPropertyName("limits")] public IList<Limit> Limits { get; private init; }
    [JsonPropertyName("retentions")] public IList<Retention> Retentions { get; private init; }
    
    public SubmissionPricing()
    {
        EnterpriseValue = new Money();
        Limits = Limit.Factory.Default;
        Retentions = Retention.Factory.Default;
    }
    public SubmissionPricing(ulong amount, Currency currency)
    {
        EnterpriseValue = new Money(amount, currency);
        Limits = Limit.Factory.Default;
        Retentions = Retention.Factory.Default;
    }

    [JsonConstructor]
    [Newtonsoft.Json.JsonConstructor]//CosmosDB SDK uses Newtonsoft for serializing
    public SubmissionPricing(Money enterpriseValue, IList<Limit> limits, IList<Retention> retentions)
    {
        if (!AreLimitsValid(limits)) throw new ArgumentException("Pricing must include at least one limit", $"{nameof(SubmissionPricing)} {nameof(limits)}");
        if (!AreRetentionsValid(retentions)) throw new ArgumentException("Pricing must include at least one retention", $"{nameof(SubmissionPricing)} {nameof(retentions)}");
        
        EnterpriseValue = enterpriseValue;
        Limits = limits;
        Retentions = retentions;
    }

    private bool AreLimitsValid(IList<Limit> limits) => limits.Any(l => l.Value > 0) && limits.Any(l => l.Enabled);
    private bool AreRetentionsValid(IList<Retention> retentions) => retentions.Where(l => l.Value > 0).Any();

    public SubmissionPricing SetEnterpriseValue(decimal amount, Currency currency)
    {
        var newEv = new Money(amount, currency);

        return new SubmissionPricing(newEv, Limits, Retentions);
    }

    public Option<SubmissionPricing, ErrorCode> SetLimits(IList<Limit> newLimits)
    {
        if (!AreLimitsValid(newLimits)) return Option.None<SubmissionPricing, ErrorCode>(PricingErrorCodes.NoLimits);

        return new SubmissionPricing(EnterpriseValue, newLimits, Retentions).Some<SubmissionPricing, ErrorCode>();
    }
    
    public Option<SubmissionPricing, ErrorCode> SetRetentions(IList<Retention> newRetentions)
    {
        if (!AreRetentionsValid(newRetentions)) return Option.None<SubmissionPricing, ErrorCode>(PricingErrorCodes.NoRetentions);

        return new SubmissionPricing(EnterpriseValue, Limits, newRetentions.OrderByDescending(r => r.Value).ToList()).Some<SubmissionPricing, ErrorCode>();
    }
}

public class FeedbackPricing : IEquatable<FeedbackPricing?>
{
    [JsonPropertyName("ev")] public Money EnterpriseValue { get; private init; }
    [JsonPropertyName("demin")] public Money DeMinimis { get; private init; }
    [JsonPropertyName("uwfee")] public Money UwFee { get; private init; }
    [JsonPropertyName("uwfeewaive")] public bool UwFeeWaiveOption { get; private init; }
    [JsonPropertyName("breakfee")] public Money BreakFee { get; private init; }
    [JsonPropertyName("breakfeewaive")] public bool BreakFeeWaiveOption { get; private init; }
    [JsonPropertyName("options")] public IList<PricingOption> Options { get; private init; }

    [Newtonsoft.Json.JsonIgnore][JsonIgnore] public IEnumerable<Limit> Limits => Options.GroupBy(o => o.Limit).Select(g => g.Key).OrderBy(l => l.Value).ToList();
    
    public FeedbackPricing()
    {
        EnterpriseValue = new Money();
        DeMinimis = new Money();
        UwFee = new Money();
        UwFeeWaiveOption = false;
        BreakFee = new Money();
        BreakFeeWaiveOption = false;
        Options = new List<PricingOption>();
    }

    [JsonConstructor]
    [Newtonsoft.Json.JsonConstructor]//CosmosDB SDK uses Newtonsoft for serializing
    public FeedbackPricing(Money enterpriseValue, Money deMinimis, Money uwFee, bool uwFeeWaiveOption, Money breakFee, bool breakFeeWaiveOption, IList<PricingOption> options)
    {
        EnterpriseValue = enterpriseValue;
        DeMinimis = deMinimis;
        UwFee = uwFee;
        UwFeeWaiveOption = uwFeeWaiveOption;
        BreakFee = breakFee;
        BreakFeeWaiveOption = breakFeeWaiveOption;
        Options = options;
    }

    public IEnumerable<PricingOption> OptionsOfLimit(int limitId) => Options.Where(o => o.Limit.Id == limitId);

    public FeedbackPricing SetPremium(PricingOption option, decimal newPremium)
    {
        var optionToUpdate = Options.Single(d => d.Limit.Value == option.Limit.Value && d.Retention == option.Retention);
        var index = Options.IndexOf(optionToUpdate);
        Options[index] = optionToUpdate.SetPremium(newPremium);

        return new FeedbackPricing(EnterpriseValue, DeMinimis, UwFee, UwFeeWaiveOption, BreakFee, BreakFeeWaiveOption, Options);
    }

    public Money GetPremium(Limit limit, Retention retention) =>
        Options.Single(o => o.Limit.Value == limit.Value && o.Retention.Value == retention.Value).Premium;

    public FeedbackPricing SetFee(string name, decimal value)
    {
        //TODO: magic strings here, refactor them out
        switch (name)
        {
            case "De Minimis":
                return new FeedbackPricing(EnterpriseValue, new Money(value, DeMinimis.Currency), UwFee, UwFeeWaiveOption, BreakFee, BreakFeeWaiveOption, Options);
            case "UW fee":
                return new FeedbackPricing(EnterpriseValue, DeMinimis, new Money(value, UwFee.Currency), UwFeeWaiveOption, BreakFee, BreakFeeWaiveOption, Options);
            case "Break fee":
                return new FeedbackPricing(EnterpriseValue, DeMinimis, UwFee, UwFeeWaiveOption, new Money(value, BreakFee.Currency), BreakFeeWaiveOption, Options);
            default:
                throw new NotImplementedException("Trying to edit a pricing value that has not been accounted for when assigning the dialog result");
        }
    }

    public FeedbackPricing SetWaive(string name, bool value)
    {
        //TODO: magic strings here, refactor them out
        switch (name)
        {
            case "UW fee":
                return new FeedbackPricing(EnterpriseValue, DeMinimis, UwFee, value, BreakFee, BreakFeeWaiveOption, Options);
            case "Break fee":
                return new FeedbackPricing(EnterpriseValue, DeMinimis, UwFee, UwFeeWaiveOption, BreakFee, value, Options);
            default:
                throw new NotImplementedException("Trying to edit a pricing value that has not been accounted for when assigning the dialog result");
        }
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as FeedbackPricing);
    }

    public bool Equals(FeedbackPricing? other)
    {
        return other is not null &&
               EqualityComparer<Money>.Default.Equals(EnterpriseValue, other.EnterpriseValue) &&
               EqualityComparer<Money>.Default.Equals(DeMinimis, other.DeMinimis) &&
               EqualityComparer<Money>.Default.Equals(UwFee, other.UwFee) &&
               UwFeeWaiveOption == other.UwFeeWaiveOption &&
               EqualityComparer<Money>.Default.Equals(BreakFee, other.BreakFee) &&
               BreakFeeWaiveOption == other.BreakFeeWaiveOption &&
               Options.SequenceEqual(other.Options);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EnterpriseValue, DeMinimis, UwFee, UwFeeWaiveOption, BreakFee, BreakFeeWaiveOption, Options);
    }

    public static bool operator ==(FeedbackPricing? left, FeedbackPricing? right)
    {
        return EqualityComparer<FeedbackPricing>.Default.Equals(left, right);
    }

    public static bool operator !=(FeedbackPricing? left, FeedbackPricing? right)
    {
        return !(left == right);
    }

    public static class Factory
    {
        public static FeedbackPricing Parse(SubmissionPricing pricing, FeedbackPricing? existingPricing = default)
        {
            if (existingPricing == null) existingPricing = new FeedbackPricing();
            var options = new List<PricingOption>();

            foreach (var limit in pricing.Limits.Where(l => l.Enabled))
            {
                foreach (var retention in pricing.Retentions.Where(l => l.Enabled))
                {
                    var premium = GetExistingPremium(existingPricing.Options, limit, retention, pricing.EnterpriseValue.Currency);
                    options.Add(new PricingOption(limit, retention, premium));
                }
            }

            return new FeedbackPricing(
                pricing.EnterpriseValue,
                existingPricing.DeMinimis,
                existingPricing.UwFee,
                existingPricing.UwFeeWaiveOption,
                existingPricing.BreakFee,
                existingPricing.BreakFeeWaiveOption,
                options
                );
        }

        private static Money GetExistingPremium(IEnumerable<PricingOption> existingOptions, Limit limit, Retention retention, Currency currency)
        {
            var existingOption = existingOptions.FirstOrDefault(o => o.Limit.Value == limit.Value && o.Retention.Value == retention.Value);
            return existingOption == null ? new Money(0m, currency) : existingOption.Premium;
        }
    }
}

public class Money : IEquatable<Money?>
{
    [JsonPropertyName("amount")] public decimal Amount { get; private init; }
    [JsonPropertyName("currency")] public Currency Currency { get; private init; }

    public Money()
    {
        Amount = 0;
        Currency = Currency.GBP;
    }

    [JsonConstructor]
    [Newtonsoft.Json.JsonConstructor]//CosmosDB SDK uses Newtonsoft for serializing
    public Money(decimal amount, Currency currency)
    {
        if (amount < 0) throw new ArgumentException("Money amount can't be negative", $"{nameof(Money)} {nameof(amount)}");

        Amount = amount;
        Currency = currency;
    }

    public string ToAmountString(string format = "N2") => Amount.ToString(format);

    public override string ToString() => 
        Amount == 0 ? 
            "Not set" :
            $"{Amount:N2} {Currency}";
    
    
    public override bool Equals(object? obj) => Equals(obj as Money);
    public bool Equals(Money? other) => 
        other is not null &&
               Amount == other.Amount &&
               Currency == other.Currency;
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public static bool operator ==(Money? left, Money? right) => EqualityComparer<Money>.Default.Equals(left, right);
    public static bool operator !=(Money? left, Money? right) => !(left == right);
}

public class Limit : IEquatable<Limit?>
{
    [JsonPropertyName("id")] public int Id { get; private init; }
    [JsonPropertyName("value")] public double Value { get; private init; }
    [JsonPropertyName("enabled")] public bool Enabled { get; private init; }

    [JsonConstructor]
    [Newtonsoft.Json.JsonConstructor]//CosmosDB SDK uses Newtonsoft for serializing
    public Limit(int id, double value, bool enabled)
    {
        if (value < 0) throw new ArgumentException("Limit value can't be negative", $"{nameof(Limit)} {nameof(value)}");

        Id = id;
        Value = value;
        Enabled = enabled;
    }

    public override string ToString() => (Value * 100).ToString("0.##");

    public decimal ToAmount(Money ev) => (decimal)Value * ev.Amount;

    public string ToAmountString(Money ev) => 
        ev.Amount == 0 ?
            "-" :
            $"{ToAmount(ev):N0} {ev.Currency}";

    
    public override bool Equals(object? obj) => Equals(obj as Limit);
    public bool Equals(Limit? other) => 
        other is not null &&
               Id == other.Id &&
               Value == other.Value &&
               Enabled == other.Enabled;
    public override int GetHashCode() => HashCode.Combine(Id, Value, Enabled);
    public static bool operator ==(Limit? left, Limit? right) => EqualityComparer<Limit>.Default.Equals(left, right);
    public static bool operator !=(Limit? left, Limit? right) => !(left == right);

    public static class Factory
    {
        public static IList<Limit> Default => new List<Limit> { 
            new Limit(0, 0.10, true), 
            new Limit(1, 0.15, true), 
            new Limit(2, 0.20, true), 
            new Limit(3, 0.25, true) };
    }
    
}

public class Retention : IEquatable<Retention?>
{
    private double val;

    [JsonPropertyName("value")] public double Value { get => val; private init => val = Math.Round(value, 5); }
    [JsonPropertyName("enabled")] public bool Enabled { get ; private init; }

    [JsonConstructor]
    [Newtonsoft.Json.JsonConstructor]//CosmosDB SDK uses Newtonsoft for serializing
    public Retention(double value, bool enabled)
    {
        if (value < 0) throw new ArgumentException("Retention value can't be negative", $"{nameof(Retention)} {nameof(value)}");

        Value = value;
        Enabled = enabled;
    }

    public override string ToString() => Value == 0 ? "nil" : (Value * 100).ToString("0.##");

    public decimal ToAmount(Money ev) => (decimal)Value * ev.Amount;

    public string ToAmountString(Money ev)
    {
        if (ev.Amount == 0) return "-";

        return Value == 0 ?
            "nil" :
            $"{ToAmount(ev):N0} {ev.Currency}";
    }

    public override bool Equals(object? obj) => Equals(obj as Retention);
    public bool Equals(Retention? other) => 
        other is not null &&
               val == other.val &&
               Enabled == other.Enabled;
    public override int GetHashCode() => HashCode.Combine(val, Enabled);
    public static bool operator ==(Retention? left, Retention? right) => EqualityComparer<Retention>.Default.Equals(left, right);
    public static bool operator !=(Retention? left, Retention? right) => !(left == right);

    public static class Factory
    {
        public static IList<Retention> Default => new List<Retention> { 
            new Retention(0.005, true), 
            new Retention(0.0035, true), 
            new Retention(0.0025, true), 
            new Retention(0, true) };
    }    
}

public class PricingOption : IEquatable<PricingOption?>
{
    [JsonPropertyName("premium")] public Money Premium { get; private init; }
    [JsonPropertyName("ret")] public Retention Retention { get; private init; }
    [JsonPropertyName("limit")] public Limit Limit { get; private init; }
    
    [JsonConstructor]
    [Newtonsoft.Json.JsonConstructor]//CosmosDB SDK uses Newtonsoft for serializing
    public PricingOption(Limit limit, Retention retention, Money premium)
    {
        Premium = premium;
        Retention = retention;
        Limit = limit;
    }

    public PricingOption SetPremium(decimal premium) => new PricingOption(Limit, Retention, new Money(premium, Premium.Currency));

    
    public override bool Equals(object? obj) => Equals(obj as PricingOption);
    
    public bool Equals(PricingOption? other) => 
        other is not null &&
        EqualityComparer<Money>.Default.Equals(Premium, other.Premium) &&
        EqualityComparer<Retention>.Default.Equals(Retention, other.Retention) &&
        EqualityComparer<Limit>.Default.Equals(Limit, other.Limit);

    public override int GetHashCode() => HashCode.Combine(Premium, Retention, Limit);
    public static bool operator ==(PricingOption? left, PricingOption? right) => EqualityComparer<PricingOption>.Default.Equals(left, right);
    public static bool operator !=(PricingOption? left, PricingOption? right) => !(left == right);
}
