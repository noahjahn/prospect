using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;
using Prospect.Server.Api.Services.UserData;

public class FYSetActiveGlobalVanityRequest
{
    [JsonPropertyName("activeVanity")]
    public FYActiveGlobalVanity ActiveVanity { get; set; }
}

public class FYSetActiveGlobalVanityResponse
{
    [JsonPropertyName("activeVanity")]
    public FYActiveGlobalVanity ActiveVanity { get; set; }
}

[CloudScriptFunction("SetActiveGlobalVanity")]
public class SetActiveGlobalVanityFunction : ICloudScriptFunction<FYSetActiveGlobalVanityRequest, FYSetActiveGlobalVanityResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;

    public SetActiveGlobalVanityFunction(IHttpContextAccessor httpContextAccessor, UserDataService userDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
    }

    public async Task<FYSetActiveGlobalVanityResponse> ExecuteAsync(FYSetActiveGlobalVanityRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        // TODO: Check vanities availability

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{ ["GlobalVanity"] = JsonSerializer.Serialize(request.ActiveVanity) }
        );

        return new FYSetActiveGlobalVanityResponse
        {
            ActiveVanity = request.ActiveVanity,
        };
    }
}
