using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class TryGetCompleteSquadInfoRequest
{
    // Empty request
}

public class FYPlayFabSquad
{
    [JsonPropertyName("squadId")]
    public string SquadID { get; set; }
    [JsonPropertyName("members")]
    public FYPlayFabSquadMember[] Members { get; set; }
}

public class FYPlayFabSquadMember {
    [JsonPropertyName("profile")]
	public FYPlayFabPlayerProfile Profile { get; set; }
    [JsonPropertyName("onlineState")]
	public int onlineState { get; set; }
    [JsonPropertyName("matchmakingSettings")]
	public FYUserMatchmakingSettings matchmakingSettings { get; set; }
    [JsonPropertyName("mapRowNamesUnlocked")]
	public string[] mapRowNamesUnlocked { get; set; }
};

public class FYPlayFabPlayerProfile {
    [JsonPropertyName("avatarUrl")]
	public string AvatarUrl { get; set; }
    [JsonPropertyName("displayName")]
	public string DisplayName { get; set; }
    [JsonPropertyName("playerId")]
	public string PlayerId { get; set; }
};

public class FYUserMatchmakingSettings {
    [JsonPropertyName("isReadyForMatch")]
	public bool isReadyForMatch;
    [JsonPropertyName("selectedMapName")]
	public string selectedMapName;
    [JsonPropertyName("isSecretLeader")]
	public bool isSecretLeader;
    [JsonPropertyName("purchaseInsuranceRequest")]
	public object purchaseInsuranceRequest; // FYPurchaseInsuranceRequest
};


[CloudScriptFunction("TryGetCompleteSquadInfo")]
public class TryGetCompleteSquadInfoFunction : ICloudScriptFunction<TryGetCompleteSquadInfoRequest, FYPlayFabSquad>
{
    private readonly ILogger<TryGetCompleteSquadInfoFunction> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TryGetCompleteSquadInfoFunction(ILogger<TryGetCompleteSquadInfoFunction> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<FYPlayFabSquad> ExecuteAsync(TryGetCompleteSquadInfoRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        // var userId = context.User.FindAuthUserId();

        return new FYPlayFabSquad
        {
            // SquadID = "100",
            // Members = [],
        };
    }
}
