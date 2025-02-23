using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models.Data;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYStartPlayerQuarterUpgradeClientRequest {
}

public class FYStartPlayerQuarterUpgradeClientResult {
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
	public List<FYCustomItemInfo> ChangedItems { get; set; }
    [JsonPropertyName("deletedItems")]
	public HashSet<string> DeletedItemsIds { get; set; }
}

[CloudScriptFunction("StartPlayerQuarterUpgradeClient")]
public class StartPlayerQuarterUpgradeClient : ICloudScriptFunction<FYStartPlayerQuarterUpgradeClientRequest, FYStartPlayerQuarterUpgradeClientResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public StartPlayerQuarterUpgradeClient(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYStartPlayerQuarterUpgradeClientResult> ExecuteAsync(FYStartPlayerQuarterUpgradeClientRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        var userData = await _userDataService.FindAsync(
            userId, userId,
            new List<string>{"PlayerQuartersLevel", "Balance", "Inventory"}
        );

        var userPlayerQuarters = JsonSerializer.Deserialize<FYPlayerQuarterStatus>(userData["PlayerQuartersLevel"].Value);
        if (userPlayerQuarters.UpgradeStartedTime.Seconds != 0) {
            return new FYStartPlayerQuarterUpgradeClientResult
            {
                UserID = userId,
                Error = "Player quarters upgrade is in progress",
            };
        }

        var titleData = _titleDataService.Find(new List<string>{"PlayerQuarters"});
        var balance = JsonSerializer.Deserialize<Dictionary<string, int>>(userData["Balance"].Value);
        var inventory = JsonSerializer.Deserialize<List<FYCustomItemInfo>>(userData["Inventory"].Value);
        var playerQuartersData = JsonSerializer.Deserialize<TitleDataPlayerQuartersInfo[]>(titleData["PlayerQuarters"]);
        // Current level contains the information about next level
        // Player quarter level starts from 1
        var nextQuartersLevel = playerQuartersData[userPlayerQuarters.Level - 1];

        // Validate price
        // TODO: Refactor
        List<FYCustomItemInfo> changedItems = [];
        HashSet<string> deletedItemsIds = [];
        List<FYCurrencyItem> changedCurrencies = [];
        foreach (var ingredient in nextQuartersLevel.UpgradeCosts) {
            int remaining = ingredient.Amount;
            if (ingredient.Currency == "SoftCurrency") {
                remaining -= ingredient.Amount;
                balance["SC"] -= ingredient.Amount;
                changedCurrencies.Add(new FYCurrencyItem { CurrencyName = "SoftCurrency", Amount = balance["SC"] });
            } else if (ingredient.Currency == "Aurum") {
                remaining -= ingredient.Amount;
                balance["AU"] -= ingredient.Amount;
                changedCurrencies.Add(new FYCurrencyItem { CurrencyName = "Aurum", Amount = balance["AU"] });
            } else {
                for (var i = 0; i < inventory.Count; i++) {
                    var item = inventory[i];
                    if (item.BaseItemId != ingredient.Currency) {
                        continue;
                    }
                    var localRemaining = remaining;
                    remaining -= item.Amount;
                    item.Amount -= localRemaining;
                    if (item.Amount <= 0) {
                        item.Amount = 0;
                        deletedItemsIds.Add(item.ItemId);
                    } else {
                        changedItems.Add(item);
                    }
                    if (remaining <= 0) {
                        break;
                    }
                }
            }
            if (remaining > 0) {
                return new FYStartPlayerQuarterUpgradeClientResult
                {
                    UserID = userId,
                    Error = "Missing required items",
                };
            }
        }

        // TODO: Optimize
        var newInventory = new List<FYCustomItemInfo>(inventory.Count);
        foreach (var item in inventory) {
            if (!deletedItemsIds.Contains(item.ItemId)) {
                newInventory.Add(item);
            }
        }

        var startTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var upgradeStartedTime = new FYTimestamp {
            Seconds = startTime
        };
        userPlayerQuarters.UpgradeStartedTime = upgradeStartedTime;

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["Balance"] = JsonSerializer.Serialize(balance),
                ["Inventory"] = JsonSerializer.Serialize(inventory),
                ["PlayerQuartersLevel"] = JsonSerializer.Serialize(userPlayerQuarters),
            }
        );

        return new FYStartPlayerQuarterUpgradeClientResult
        {
            UserID = userId,
            Error = "",
            NewLevel = userPlayerQuarters.Level++,
            UpgradeStartedTime = upgradeStartedTime,
            RemainingTimeInSeconds = nextQuartersLevel.UpgradeSeconds,
            ChangedItems = changedItems,
            DeletedItemsIds = deletedItemsIds,
            ChangedCurrencies = changedCurrencies.ToArray(),
        };
    }
}