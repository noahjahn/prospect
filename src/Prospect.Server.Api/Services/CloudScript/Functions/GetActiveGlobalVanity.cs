using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;
using Prospect.Server.Api.Services.UserData;

public class FYActiveGlobalVanity {
    [JsonPropertyName("activeGlobalVanityIds")]
    public string[] ActiveGlobalVanityIds { get; set; }
    [JsonPropertyName("droppodId")]
    public string DropPodID { get; set; }
}

public class FYGetActiveGlobalVanityRequest
{
    // Empty request
}

public class FYGetActiveGlobalVanityResponse
{
    [JsonPropertyName("activeVanity")]
    public FYActiveGlobalVanity ActiveVanity { get; set; }
}

[CloudScriptFunction("GetActiveGlobalVanity")]
public class GetActiveGlobalVanityFunction : ICloudScriptFunction<FYGetActiveGlobalVanityRequest, FYGetActiveGlobalVanityResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;

    public GetActiveGlobalVanityFunction(IHttpContextAccessor httpContextAccessor, UserDataService userDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
    }

    public async Task<FYGetActiveGlobalVanityResponse> ExecuteAsync(FYGetActiveGlobalVanityRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"GlobalVanity"});
        var activeVanity = JsonSerializer.Deserialize<FYActiveGlobalVanity>(userData["GlobalVanity"].Value);

        return new FYGetActiveGlobalVanityResponse{
            ActiveVanity = activeVanity
        };
    }
}
