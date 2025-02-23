using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models.Data;
using Prospect.Server.Api.Services.UserData;
using Prospect.Server.Api.Utils;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYSkipPlayerQuarterUpgradeClientRequest {
    [JsonPropertyName("useOptionalCosts")]
	public bool UseOptionalCosts { get; set; }
}

public class FYSkipPlayerQuarterUpgradeClientResult {
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

[CloudScriptFunction("SkipPlayerQuarterUpgradeClient")]
public class SkipPlayerQuarterUpgradeClient : ICloudScriptFunction<FYSkipPlayerQuarterUpgradeClientRequest, FYSkipPlayerQuarterUpgradeClientResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public SkipPlayerQuarterUpgradeClient(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYSkipPlayerQuarterUpgradeClientResult> ExecuteAsync(FYSkipPlayerQuarterUpgradeClientRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        var userData = await _userDataService.FindAsync(
            userId, userId,
            new List<string>{"PlayerQuartersLevel", "Balance"}
        );

        var userPlayerQuarters = JsonSerializer.Deserialize<FYPlayerQuarterStatus>(userData["PlayerQuartersLevel"].Value);
        if (userPlayerQuarters.UpgradeStartedTime.Seconds == 0) {
            return new FYSkipPlayerQuarterUpgradeClientResult
            {
                UserID = userId,
                Error = "Nothing is being constructed",
            };
        }

        var titleData = _titleDataService.Find(new List<string>{"PlayerQuarters"});
        var balance = JsonSerializer.Deserialize<Dictionary<string, int>>(userData["Balance"].Value);
        var playerQuartersData = JsonSerializer.Deserialize<TitleDataPlayerQuartersInfo[]>(titleData["PlayerQuarters"]);
        var nextQuartersLevel = playerQuartersData[userPlayerQuarters.Level - 1]; // Quarters level starts from 1

        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var craftEndTime = userPlayerQuarters.UpgradeStartedTime.Seconds + nextQuartersLevel.UpgradeSeconds;
        var craftStartTime = userPlayerQuarters.UpgradeStartedTime.Seconds;
        FYCurrencyItem[] changedCurrency;
        if (request.UseOptionalCosts) {
            var remaining = MapValue.Map(now, craftStartTime, craftEndTime, nextQuartersLevel.OptionalRushCosts, 1);
            if (balance["SC"] < remaining) {
                return new FYSkipPlayerQuarterUpgradeClientResult {
                    UserID = userId,
                    Error = "Insufficient balance",
                };
            }
            balance["SC"] -= remaining;
            changedCurrency = [new FYCurrencyItem { CurrencyName = "SoftCurrency", Amount = balance["SC"] }];
        } else {
            var remaining = MapValue.Map(now, craftStartTime, craftEndTime, nextQuartersLevel.InitialRushCosts, 1);
            if (balance["AU"] < remaining) {
                return new FYSkipPlayerQuarterUpgradeClientResult {
                    UserID = userId,
                    Error = "Insufficient balance",
                };
            }
            balance["AU"] -= remaining;
            changedCurrency = [new FYCurrencyItem { CurrencyName = "Aurum", Amount = balance["AU"] }];
        }

        var upgradeStartedTime = new FYTimestamp {
            Seconds = 0,
        };
        userPlayerQuarters.Level++;
        userPlayerQuarters.UpgradeStartedTime = upgradeStartedTime;

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["Balance"] = JsonSerializer.Serialize(balance),
                ["PlayerQuartersLevel"] = JsonSerializer.Serialize(userPlayerQuarters),
            }
        );

        return new FYSkipPlayerQuarterUpgradeClientResult
        {
            UserID = userId,
            Error = "",
            NewLevel = userPlayerQuarters.Level,
            UpgradeStartedTime = upgradeStartedTime,
            RemainingTimeInSeconds = 0,
            ChangedItems = [],
            DeletedItemsIds = [],
            ChangedCurrencies = changedCurrency,
        };
    }
}