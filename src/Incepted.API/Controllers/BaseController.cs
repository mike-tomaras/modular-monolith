using Microsoft.AspNetCore.Mvc;
using Incepted.Shared;
using System.Diagnostics;
using Incepted.Shared.ValueTypes;

namespace Incepted.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class BaseController : ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserId UserId => _httpContextAccessor
                .GetAuthIdFromAccessToken()
                .ValueOr(() => throw new InvalidOperationException("There is no auth Id and/or access token. " +
                    "Authenticate and try again."));

    public BaseController(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    internal ObjectResult EnrichedError(ErrorCode errorCode)
    {
        var errorWitTraceId = errorCode with { traceId = Activity.Current?.Id ?? Request.HttpContext.TraceIdentifier };

        ObjectResult result = errorWitTraceId.status switch
        {
            400 => new BadRequestObjectResult(errorCode),
            404 => new NotFoundObjectResult(errorCode),
            500 => new ObjectResult(errorCode) { StatusCode = 500 },
            _ => throw new ArgumentException("Failed to return enrich error, the status code is not mapped.")
        };
        
        return result;
    }
}