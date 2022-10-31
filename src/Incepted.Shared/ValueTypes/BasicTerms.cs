using Incepted.Shared.Enums;
using System.Text.Json.Serialization;

namespace Incepted.Shared.ValueTypes;

public record class BasicTerms
{
    //the setters are public on purpose as there are no business rules about these props
    [JsonPropertyName("insBuyer")] public string InsuredAndBuyer { get; set; } = string.Empty;
    [JsonPropertyName("insBuyerJur")] public Jurisdiction InsuredAndBuyerJurisdiction { get; set; }
    [JsonPropertyName("target")] public string Target { get; set; } = string.Empty;
    [JsonPropertyName("targetJur")] public Jurisdiction TargetJurisdiction { get; set; }
    [JsonPropertyName("ubo")] public string UBO { get; set; } = string.Empty;
    [JsonPropertyName("uboJur")] public Jurisdiction UBOJurisdiction { get; set; }
    [JsonPropertyName("seller")] public string Sellers { get; set; } = string.Empty;
    [JsonPropertyName("process")] public DealProcess Process { get; set; }
    [JsonPropertyName("industry")] public string Industry { get; set; } = string.Empty;
    [JsonPropertyName("targetDesc")] public string TargetShortDescription { get; set; } = string.Empty;
    [JsonPropertyName("finInfo")] public string FinancialInfo { get; set; } = string.Empty;
    [JsonPropertyName("geoFootprint")] public string GeographicalFoorprint { get; set; } = string.Empty;
    [JsonPropertyName("govLaw")] public Jurisdiction GoverningLaw { get; set; }
    [JsonPropertyName("noOfEmpl")] public int EmployeesNumber { get; set; }
    [JsonPropertyName("purchacePriceMech")] public PurchasePriceMechanism PurchasePriceMechanism { get; set; }
    [JsonPropertyName("insObligations")] public string InsuredObligations { get; set; } = string.Empty;
    [JsonPropertyName("policyDurationBusinessWarranties")] public int PolicyDurationInMonthsForBusinessWarranties { get; set; } = 24;
    [JsonPropertyName("policyDurationFundamentalWarranties")] public int PolicyDurationInMonthsForFundamentalWarranties { get; set; } = 84;
    [JsonPropertyName("policyDurationTaxIdemnity")] public int PolicyDurationInMonthsForTaxIdemnity { get; set; } = 84;
    [JsonPropertyName("buyAdv")] public List<DealAdvisor> BuySideAdvisors { get; set; } = new List<DealAdvisor>
    {
        new DealAdvisor { Type = "Legal" },
        new DealAdvisor { Type = "Tax and Accounting" },
        new DealAdvisor { Type = "Commercial/Technical" },
        new DealAdvisor { Type = "Financial" },
    };
    [JsonPropertyName("sellAdv")] public List<DealAdvisor> SellSideAdvisors { get; set; } = new List<DealAdvisor>
    {
        new DealAdvisor { Type = "Legal" },
        new DealAdvisor { Type = "Tax and Accounting" },
        new DealAdvisor { Type = "Commercial/Technical" },
        new DealAdvisor { Type = "Financial" },
    };
    [JsonPropertyName("bidDate")] public DateTime? BidDate { get; set; } = DateTime.Now;
    [JsonPropertyName("signDate")] public DateTime? SigningDate { get; set; } = DateTime.Now;
    [JsonPropertyName("finalPolicyDate")] public DateTime? FinalPolicyDate { get; set; } = DateTime.Now;
    [JsonPropertyName("notes")] public string Notes { get; set; } = string.Empty;
}

public class DealAdvisor : IEquatable<DealAdvisor?>
{
    //Full properties to override nulls to empty strings. Mudblazor renders nulls as empty strings
    //but only saves empty string when no value is entered so this way we can preserve equality.
    private string name;
    private string type;

    [JsonPropertyName("advName")] public string Name { get => name; set => name = value == null ? string.Empty : value; }
    [JsonPropertyName("advType")] public string Type { get => type; set => type = value == null ? string.Empty : value; }

    public override bool Equals(object? obj) => Equals(obj as DealAdvisor);
    public bool Equals(DealAdvisor? other) => 
        other is not null &&
               name == other.name &&
               type == other.type;
    public override int GetHashCode() => HashCode.Combine(name, type);
    public static bool operator ==(DealAdvisor? left, DealAdvisor? right) => EqualityComparer<DealAdvisor>.Default.Equals(left, right);
    public static bool operator !=(DealAdvisor? left, DealAdvisor? right) => !(left == right);
}
