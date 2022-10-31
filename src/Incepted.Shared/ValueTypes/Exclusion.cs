using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Incepted.Shared.ValueTypes;

public class Exclusion : IEquatable<Exclusion?>
{
    [JsonPropertyName("title")] public string Title { get; private init; }
    [JsonPropertyName("description")] public string Description { get; private init; }
    [JsonPropertyName("insurerRequiresIt")] public bool InsurerRequiresIt { get; private init; }
    [JsonPropertyName("comment")] public string Comment { get; private init; } = string.Empty;


    [Newtonsoft.Json.JsonIgnore][JsonIgnore] public bool HasComment => !string.IsNullOrEmpty(Comment);

    public Exclusion(string title, string description)
    {
        if (string.IsNullOrEmpty(title)) throw new ArgumentException("Exclusion title can't be empty", $"{nameof(Exclusion)} {nameof(title)}");
        
        Title = title;
        Description = description;
        InsurerRequiresIt = false;
        Comment = string.Empty;
    }

    [JsonConstructor]
    [Newtonsoft.Json.JsonConstructor]//CosmosDB SDK uses Newtonsoft for serializing
    public Exclusion(string title, string description, string comment, bool insurerRequiresIt)
    {
        if (string.IsNullOrEmpty(title)) throw new ArgumentException("Exclusion title can't be empty", $"{nameof(Exclusion)} {nameof(title)}");
        
        Title = title;
        Description = description;
        Comment = comment;
        InsurerRequiresIt = insurerRequiresIt;
    }

    public Exclusion SetInsurerSelected(bool value) => new Exclusion(Title, Description, Comment, value);
    public Exclusion SetComment(string comment)
    {
        var insurerSelected = !string.IsNullOrEmpty(comment) ? true : InsurerRequiresIt;

        return new Exclusion(Title, Description, comment, insurerSelected);
    }

    public override bool Equals(object? obj) => Equals(obj as Exclusion);
    public bool Equals(Exclusion? other) => 
        other is not null &&
               Title == other.Title &&
               Description == other.Description &&
               Comment == other.Comment &&
               InsurerRequiresIt == other.InsurerRequiresIt;
    public override int GetHashCode() => HashCode.Combine(Title, Description, Comment, InsurerRequiresIt);
    public static bool operator ==(Exclusion? left, Exclusion? right) => EqualityComparer<Exclusion>.Default.Equals(left, right);
    public static bool operator !=(Exclusion? left, Exclusion? right) => !(left == right);

    public static class Factory
    {
        public static IImmutableList<Exclusion> Default => ImmutableList.CreateRange(new List<Exclusion>
        {
            new Exclusion("Non written agreements, notices and communication", ""),
            new Exclusion("Compliance with Laws", ""),
            new Exclusion("Secondary Tax Liability", ""),
            new Exclusion("Transfer pricing", ""),
            new Exclusion("Availability of tax losses", ""),
            new Exclusion("Bribery, Corruption, Money Laundering", ""),
            new Exclusion("Condition of Assets / Real Estate Property", ""),
            new Exclusion("Cyber / IT", ""),
            new Exclusion("Regulatory compliance", ""),
            new Exclusion("Data protection cyber security", ""),
            new Exclusion("Cyber security", ""),
            new Exclusion("Professional Liability", "Defects, failures in products/ services and software malfunctioning"),
            new Exclusion("Re-qualification of employees / freelancers", ""),
            new Exclusion("Holiday pay", ""),
            new Exclusion("Pollution", ""),
            new Exclusion("Asbestos and PCBs", ""),
            new Exclusion("Environmental", ""),
            new Exclusion("COVID-19", ""),
            new Exclusion("Sanctions limitations", "Please define exact scope of exclusion"),
            new Exclusion("Pension underfunding", "Please define exact scope of exclusion, i.e. suffiency of accruals, premia and contributions paid etc."),
            new Exclusion("Product Liability", "")
        });
    }    
}
