using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models.Data;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYPlayerQuarterUpgradePurchaseClientRequest {
    // Empty request
}

public class FYPlayerQuarterUpgradePurchaseClientResult {
    [JsonPropertyName("userId")]
	public string UserID { get; set; }
    [JsonPropertyName("error")]
	public string Error { get; set; }
    [JsonPropertyName("newLevel")]
	public int NewLevel { get; set; }
    [JsonPropertyName("upgradeStartedTime")]
	public FYTimestamp UpgradeStartedTime { get; set; }
    [JsonPropertyName("remainingTimeInSeconds")]
	public int RemainingTimeInSeconds { get; set; }
    [JsonPropertyName("changedCurrencies")]
	public FYCurrencyItem[] ChangedCurrencies { get; set; }
    [JsonPropertyName("changedItems")]
	public FYCustomItemInfo[] ChangedItems { get; set; }
    [JsonPropertyName("deletedItems")]
	public string[] DeletedItemsIds { get; set; }
}

[CloudScriptFunction("PlayerQuarterUpgradePurchaseClient")]
public class PlayerQuarterUpgradePurchaseClient : ICloudScriptFunction<FYPlayerQuarterUpgradePurchaseClientRequest, FYPlayerQuarterUpgradePurchaseClientResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public PlayerQuarterUpgradePurchaseClient(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYPlayerQuarterUpgradePurchaseClientResult> ExecuteAsync(FYPlayerQuarterUpgradePurchaseClientRequest request)
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

        var userPlayerQuarters = JsonSerializer.Deserialize<FYPlayerQuarterStatus>(userData["PlayerQuartersLevel"].Value);
        var titleData = _titleDataService.Find(new List<string>{"PlayerQuarters"});
        var playerQuarters = JsonSerializer.Deserialize<TitleDataPlayerQuartersInfo[]>(titleData["PlayerQuarters"]);

        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var endTime = playerQuarters[userPlayerQuarters.Level - 1].UpgradeSeconds + userPlayerQuarters.UpgradeStartedTime.Seconds;
        if (now < endTime) {
            return new FYPlayerQuarterUpgradePurchaseClientResult
            {
                UserID = userId,
                Error = "Upgrade is in progress",
            };
        }

        var upgradeStartedTime = new FYTimestamp {
            Seconds = 0,
        };
        userPlayerQuarters.Level++;
        userPlayerQuarters.UpgradeStartedTime = upgradeStartedTime;

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["PlayerQuartersLevel"] = JsonSerializer.Serialize(userPlayerQuarters),
            }
        );

        return new FYPlayerQuarterUpgradePurchaseClientResult
        {
            UserID = userId,
            Error = "",
            NewLevel = userPlayerQuarters.Level,
            UpgradeStartedTime = upgradeStartedTime,
            RemainingTimeInSeconds = 0,
            ChangedItems = [],
            DeletedItemsIds = [],
            ChangedCurrencies = [],
        };
    }
}