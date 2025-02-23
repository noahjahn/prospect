using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models.Data;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYStartTechTreeNodeUpgradeClientRequest {
    [JsonPropertyName("nodeId")]
    public string NodeID { get; set; }
}

public class FYStartTechTreeNodeUpgradeClientResult {
    [JsonPropertyName("userId")]
	public string UserID { get; set; }
    [JsonPropertyName("error")]
	public string Error { get; set; }
    [JsonPropertyName("upgradedNode")]
	public FYTechTreeNodeStatus UpgradedNode { get; set; }
    [JsonPropertyName("remainingTimeInSeconds")]
	public int RemainingTimeInSeconds { get; set; }
    [JsonPropertyName("changedCurrencies")]
	public FYCurrencyItem[] ChangedCurrencies { get; set; }
    [JsonPropertyName("changedItems")]
	public List<FYCustomItemInfo> ChangedItems { get; set; }
    [JsonPropertyName("deletedItems")]
	public HashSet<string> DeletedItemsIds { get; set; }
}

[CloudScriptFunction("StartTechTreeNodeUpgradeClient")]
public class StartTechTreeNodeUpgradeClient : ICloudScriptFunction<FYStartTechTreeNodeUpgradeClientRequest, FYStartTechTreeNodeUpgradeClientResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public StartTechTreeNodeUpgradeClient(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYStartTechTreeNodeUpgradeClientResult> ExecuteAsync(FYStartTechTreeNodeUpgradeClientRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        var userData = await _userDataService.FindAsync(
            userId, userId,
            new List<string>{"TechTreeNodeData", "Balance", "Inventory"}
        );

        var userTechTree = JsonSerializer.Deserialize<UserTechTreeNodeData>(userData["TechTreeNodeData"].Value);

        var currentNode = userTechTree.NodeInProgress;
        if (currentNode != "") {
            return new FYStartTechTreeNodeUpgradeClientResult
            {
                UserID = userId,
                Error = $"{currentNode} is in progress",
            };
        }

        var balance = JsonSerializer.Deserialize<Dictionary<string, int>>(userData["Balance"].Value);
        var inventory = JsonSerializer.Deserialize<List<FYCustomItemInfo>>(userData["Inventory"].Value);

        var titleData = _titleDataService.Find(new List<string>{"TechTreeNodes"});
        var techTreeNodes = JsonSerializer.Deserialize<Dictionary<string, TitleDataTechTreeNodeInfo>>(titleData["TechTreeNodes"]);

        // Current level contains the information about next level
        var currentLevel = userTechTree.Nodes[request.NodeID].Level;
        var techTreeNodeData = techTreeNodes[request.NodeID];
        var nextTreeNodeLevel = techTreeNodeData.PerkLevels[currentLevel]; // Tech tree node levels start from 0

        // TODO: Dependency check

        // Validate price
        // TODO: Refactor
        List<FYCustomItemInfo> changedItems = [];
        HashSet<string> deletedItemsIds = [];
        List<FYCurrencyItem> changedCurrencies = [];
        foreach (var ingredient in nextTreeNodeLevel.UpgradeCosts) {
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
                return new FYStartTechTreeNodeUpgradeClientResult
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
        userTechTree.NodeInProgress = request.NodeID;
        var upgradeStartedTime = new FYTimestamp {
            Seconds = startTime
        };
        var upgradedNode = new FYTechTreeNodeStatus {
            Level = currentLevel,
            NodeID = request.NodeID,
            UpgradeStartedTime = upgradeStartedTime,
        };
        userTechTree.Nodes[request.NodeID] = upgradedNode;

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["Balance"] = JsonSerializer.Serialize(balance),
                ["Inventory"] = JsonSerializer.Serialize(newInventory),
                ["TechTreeNodeData"] = JsonSerializer.Serialize(userTechTree),
            }
        );

        return new FYStartTechTreeNodeUpgradeClientResult
        {
            UserID = userId,
            Error = "",
            UpgradedNode = upgradedNode,
            RemainingTimeInSeconds = nextTreeNodeLevel.UpgradeSeconds,
            ChangedItems = changedItems,
            DeletedItemsIds = deletedItemsIds,
            ChangedCurrencies = changedCurrencies.ToArray(),
        };
    }
}