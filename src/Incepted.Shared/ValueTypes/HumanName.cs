using Optional;
using System.Text.Json.Serialization;

namespace Incepted.Shared.ValueTypes;

public  class HumanName
{
    [JsonPropertyName("first")] public string First { get; private init; }
    [JsonPropertyName("last")] public string Last { get; private init; }

    public HumanName(string firstName, string lastName)
    {
        if (string.IsNullOrEmpty(firstName)) throw new ArgumentException("First name can't be empty", nameof(firstName));
        if (string.IsNullOrEmpty(lastName)) throw new ArgumentException("Last name can't be empty", nameof(lastName));

        First = firstName;
        Last = lastName;
    }

    public Option<HumanName, ErrorCode> Update(string firstName, string lastName)
    {
        if (string.IsNullOrEmpty(firstName)) return Option.None<HumanName, ErrorCode>(CompanyErrorCodes.FirstNameEmpty);
        if (string.IsNullOrEmpty(lastName)) return Option.None<HumanName, ErrorCode>(CompanyErrorCodes.LastNameEmpty);

        return new HumanName(firstName, lastName).Some<HumanName, ErrorCode>();
    }
}
