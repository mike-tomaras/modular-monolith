using Incepted.Shared.DTOs;
using Optional;
using System.Security.Claims;

namespace Incepted.Client.Extensions;

internal static class ClaimExtensions
{
    public static Option<string> GetUserId(this IEnumerable<Claim> claims)
    {
        return claims.GetClaimValue("sub");
    }

    public static string GetAvatar(this IEnumerable<Claim> claims)
    {
        return claims.GetClaimValue("picture").ValueOr("http://www.gravatar.com/avatar/?d=identicon");
    }

    public static string GetName(this IEnumerable<Claim> claims)
    {
        var aaa = claims.GetUserMetadata();

        return claims.GetUserMetadata()
                .Map(x => x.FirstName).ValueOr(
                    claims.GetClaimValue("given_name").ValueOr(
                        claims.GetClaimValue("name").ValueOr(
                            claims.GetClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").ValueOr("")
                            )
                        )
                    );
    }

    public static Option<string> GetClaimValue(this IEnumerable<Claim> claims, string claimName)
    {
        var claim = claims.FirstOrDefault(c => c.Type == claimName);
        if (claim != null && claim.Value != "[]") return claim.Value.Some();

        return Option.None<string>();
    }

    public static Option<UserDTO> GetUserMetadata(this IEnumerable<Claim> claims)
    {
        var userData = new UserDTO("empty", "empty", "empty");

        return claims.GetClaimValue("https://incepted.co.uk/user_metadata")
            .FlatMap(claim => JsonExtensions.DeserializeJson<UserDTO>(claim))
            .Map(data => userData = data)
            .FlatMap(_ => claims.GetClaimValue(ClaimTypes.Email))
            .Map(email => userData with { Email = email });
    }


}
