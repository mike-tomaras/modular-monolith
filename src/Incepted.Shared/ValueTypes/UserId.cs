namespace Incepted.Shared.ValueTypes;

public record class UserId
{
    public string Value { get; private init; }

    public UserId(string userIdString)
    {
        if (string.IsNullOrEmpty(userIdString) ||
            !userIdString.Contains("|") ||
            userIdString.Split('|').First() != "auth0")
            throw new ArgumentException("UserId string is not in the correct format.", nameof(userIdString));

        Value = userIdString;
    }

    public override string ToString() => Value;
}
