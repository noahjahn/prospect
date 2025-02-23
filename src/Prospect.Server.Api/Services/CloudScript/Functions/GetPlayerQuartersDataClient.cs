using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;
using Prospect.Server.Api.Services.CloudScript.Models.Data;
using Prospect.Server.Api.Services.UserData;

public class GetPlayerQuartersDataClientRequest
{
    // Empty request
}

public class FYGetPlayerQuartersDataClientResponse
{
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("level")]
    public int Level { get; set; }
    [JsonPropertyName("upgradeStartedTime")]
	public FYTimestamp UpgradeStartedTime { get; set; }
    [JsonPropertyName("remainingTimeInSeconds")]
	public int RemainingTimeInSeconds { get; set; }
}

public class FYPlayerQuarterStatus
{
    [JsonPropertyName("level")]
    public int Level { get; set; }
    [JsonPropertyName("upgradeStartedTime")]
	public FYTimestamp UpgradeStartedTime { get; set; }
}

[CloudScriptFunction("GetPlayerQuartersDataClient")]
public class GetPlayerQuartersDataClientFunction : ICloudScriptFunction<GetPlayerQuartersDataClientRequest, FYGetPlayerQuartersDataClientResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public GetPlayerQuartersDataClientFunction(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYGetPlayerQuartersDataClientResponse> ExecuteAsync(GetPlayerQuartersDataClientRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(
            userId, userId,
            new List<string>{"PlayerQuartersLevel"}
        );
        var titleData = _titleDataService.Find(new List<string>{ "PlayerQuarters" });

        var playerQuartersData = JsonSerializer.Deserialize<TitleDataPlayerQuartersInfo[]>(titleData["PlayerQuarters"]);
        var userPlayerQuarters = JsonSerializer.Deserialize<FYPlayerQuarterStatus>(userData["PlayerQuartersLevel"].Value);
        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var remainingTime = 0;
        if (userPlayerQuarters.UpgradeStartedTime.Seconds != 0) {
            var nextLevel = playerQuartersData[userPlayerQuarters.Level - 1];
            remainingTime = userPlayerQuarters.UpgradeStartedTime.Seconds + nextLevel.UpgradeSeconds - now;
        }

        return new FYGetPlayerQuartersDataClientResponse {
            UserID = userId,
            Level = userPlayerQuarters.Level,
            RemainingTimeInSeconds = remainingTime,
            UpgradeStartedTime = userPlayerQuarters.UpgradeStartedTime,
        };
    }
}
