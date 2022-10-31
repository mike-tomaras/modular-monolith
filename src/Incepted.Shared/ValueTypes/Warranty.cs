using Incepted.Shared.Enums;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Incepted.Shared.ValueTypes;

public class Warranty : IEquatable<Warranty?>
{
    [JsonPropertyName("order")] public uint Order { get; private init; }
    [JsonPropertyName("description")] public string Description { get; private init; }
    [JsonPropertyName("coveragePos")] public CoveragePosition CoveragePosition { get; private init; }
    [JsonPropertyName("knowlScrape")] public KnowledgeScrape KnowledgeScrape { get; private init; }
    [JsonPropertyName("comment")] public string Comment { get; private init; } = string.Empty;

    [Newtonsoft.Json.JsonIgnore][JsonIgnore] public bool HasComment => !string.IsNullOrEmpty(Comment);

    public Warranty(uint order, string description)
    {
        if (string.IsNullOrEmpty(description)) throw new ArgumentException("Warranty description can't be empty", $"{nameof(Warranty)} {nameof(description)}");

        Order = order;
        Description = description;
    }

    [JsonConstructor]
    [Newtonsoft.Json.JsonConstructor]//CosmosDB SDK uses Newtonsoft for serializing
    public Warranty(uint order, string description, CoveragePosition coveragePosition, KnowledgeScrape knowledgeScrape, string comment)
    {
        if (string.IsNullOrEmpty(description)) throw new ArgumentException("Warranty description can't be empty", $"{nameof(Warranty)} {nameof(description)}");

        Order = order;
        Description = description;
        CoveragePosition = coveragePosition;
        KnowledgeScrape = knowledgeScrape;
        Comment = comment;        
    }

    public Warranty SetCoveragePosition(CoveragePosition coveragePosition) =>
        new Warranty(Order, Description, coveragePosition, KnowledgeScrape, Comment);

    public Warranty SetKnowledgeScrape(KnowledgeScrape knowledgeScrape) =>
        new Warranty(Order, Description, CoveragePosition, knowledgeScrape, Comment);

    public Warranty SetComment(string comment) => 
        new Warranty(Order, Description, CoveragePosition, KnowledgeScrape, comment);

    public override bool Equals(object? obj) => Equals(obj as Warranty);
    public bool Equals(Warranty? other) => 
        other is not null &&
               Order == other.Order &&
               Description == other.Description &&
               CoveragePosition == other.CoveragePosition &&
               KnowledgeScrape == other.KnowledgeScrape &&
               Comment == other.Comment;
    public override int GetHashCode() => HashCode.Combine(Order, Description, CoveragePosition, KnowledgeScrape, Comment);
    public static bool operator ==(Warranty? left, Warranty? right) => EqualityComparer<Warranty>.Default.Equals(left, right);
    public static bool operator !=(Warranty? left, Warranty? right) => !(left == right);

    public static class Factory
    {
        public static IImmutableList<Warranty> Default => ImmutableList.CreateRange(new List<Warranty>
        {
            new Warranty(1, "11.2 Seller's Guarantees"),
            new Warranty(2, "11.2.1 enforceability, capacity"),
            new Warranty(3, "11.2.1.1"),
            new Warranty(4, "11.2.1.2"),
            new Warranty(5, "11.2.2 existence of the amls group companies"),
            new Warranty(6, "11.2.3 ownership of the shares"),
            new Warranty(7, "11.2.5 insolvency proceedings "),
            new Warranty(8, "11.2.6 enterprise agreements"),
            new Warranty(9, "11.2.7 financial information  "),
            new Warranty(10, "11.2.7.1"),
            new Warranty(11, "11.2.7.2"),
            new Warranty(12, "11.2.7.3")
        });
    }    
}
