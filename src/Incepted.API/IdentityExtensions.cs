using Incepted.Shared.ValueTypes;
using Optional;
using Serilog;
using System.Security.Claims;

namespace Incepted.API;

internal static class IdentityExtensions
{
    public static Option<UserId> GetAuthIdFromAccessToken(this IHttpContextAccessor contextAccessor)
    {
        var authId = contextAccessor.HttpContext?
                        .User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?
                        .Value;

        if (authId == null)
        {
            var existingClaimNames = contextAccessor.HttpContext?.User.Claims
                                        .Aggregate(string.Empty, (current, next) => $"{current}, {next.Type}={next.Value}")
                                        .TrimStart(',')
                                        .TrimStart();
            Log.Warning("Attempt to access the API with no valid Auth Id Claim. {ClaimNames}", existingClaimNames);
            return Option.None<UserId>();
        }
        
        return new UserId(authId).Some();
    }
}

