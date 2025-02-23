using Prospect.Server.Api.Services.CloudScript;

public class FetchSteamEntitlementsRequest
{
    // Empty request
}

public class FetchSteamEntitlementsResponse
{
    // TODO: Unknown structure
}

[CloudScriptFunction("FetchSteamEntitlements")]
public class FetchSteamEntitlementsFunction : ICloudScriptFunction<FetchSteamEntitlementsRequest, FetchSteamEntitlementsResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FetchSteamEntitlementsFunction(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FetchSteamEntitlementsResponse> ExecuteAsync(FetchSteamEntitlementsRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }

        return new FetchSteamEntitlementsResponse
        {};
    }
}
