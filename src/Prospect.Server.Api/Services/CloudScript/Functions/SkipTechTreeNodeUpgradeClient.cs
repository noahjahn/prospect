using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.UserData;
using Prospect.Server.Api.Utils;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYSkipTechTreeNodeUpgradeClientRequest {
    [JsonPropertyName("useOptionalCosts")]
	public bool UseOptionalCosts { get; set; }
}

public class FYSkipTechTreeNodeUpgradeClientResult {
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
	public FYCustomItemInfo[] ChangedItems { get; set; }
    [JsonPropertyName("deletedItems")]
	public string[] DeletedItemsIds { get; set; }
}

[CloudScriptFunction("SkipTechTreeNodeUpgradeClient")]
public class SkipTechTreeNodeUpgradeClient : ICloudScriptFunction<FYSkipTechTreeNodeUpgradeClientRequest, FYSkipTechTreeNodeUpgradeClientResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public SkipTechTreeNodeUpgradeClient(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYSkipTechTreeNodeUpgradeClientResult> ExecuteAsync(FYSkipTechTreeNodeUpgradeClientRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        var userData = await _userDataService.FindAsync(
            userId, userId,
            new List<string>{"TechTreeNodeData", "Balance", "CharacterTechTreeBonuses"}
        );

        var balance = JsonSerializer.Deserialize<Dictionary<string, int>>(userData["Balance"].Value);
        var userTechTree = JsonSerializer.Deserialize<UserTechTreeNodeData>(userData["TechTreeNodeData"].Value);

        var currentNode = userTechTree.NodeInProgress;
        if (currentNode == "") {
            return new FYSkipTechTreeNodeUpgradeClientResult
            {
                UserID = userId,
                Error = "Nothing is being constructed",
            };
        }

        var titleData = _titleDataService.Find(new List<string>{"TechTreeNodes"});
        var techTreeNodes = JsonSerializer.Deserialize<Dictionary<string, TitleDataTechTreeNodeInfo>>(titleData["TechTreeNodes"]);

        var node = userTechTree.Nodes[currentNode];
        var nodeInfo = techTreeNodes[currentNode];
        var nodeLevelInfo = techTreeNodes[currentNode].PerkLevels[node.Level]; // Current node level - next upgradable level due to index offset

        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var craftEndTime = node.UpgradeStartedTime.Seconds + nodeLevelInfo.UpgradeSeconds;
        var craftStartTime = node.UpgradeStartedTime.Seconds;
        FYCurrencyItem[] changedCurrency;
        if (request.UseOptionalCosts) {
            var remaining = MapValue.Map(now, craftStartTime, craftEndTime, nodeLevelInfo.OptionalRushCosts, 1);
            if (balance["SC"] < remaining) {
                return new FYSkipTechTreeNodeUpgradeClientResult {
                    UserID = userId,
                    Error = "Insufficient balance",
                };
            }
            balance["SC"] -= remaining;
            changedCurrency = [new FYCurrencyItem { CurrencyName = "SoftCurrency", Amount = balance["SC"] }];
        } else {
            var remaining = MapValue.Map(now, craftStartTime, craftEndTime, nodeLevelInfo.InitialRushCosts, 1);
            if (balance["AU"] < remaining) {
                return new FYSkipTechTreeNodeUpgradeClientResult {
                    UserID = userId,
                    Error = "Insufficient balance",
                };
            }
            balance["AU"] -= remaining;
            changedCurrency = [new FYCurrencyItem { CurrencyName = "Aurum", Amount = balance["AU"] }];
        }

        userTechTree.TotalUpgrades++;
        userTechTree.NodeInProgress = "";
        node.Level++;
        node.UpgradeStartedTime.Seconds = 0;

        var techTreeBonuses = JsonSerializer.Deserialize<CharacterTechTreeBonuses>(userData["CharacterTechTreeBonuses"].Value);
        switch (nodeInfo.NodePerkType) {
            case EYTechTreeNodePerkType.DailyCrate:
                techTreeBonuses.CrateTier += (int)nodeLevelInfo.PerkAmount;
                break;
            case EYTechTreeNodePerkType.IncreaseStashSize:
                techTreeBonuses.AddStashSize += (int)nodeLevelInfo.PerkAmount;
                break;
            case EYTechTreeNodePerkType.IncreaseBagSize:
                techTreeBonuses.AddSafePocketSize += (int)nodeLevelInfo.PerkAmount;
                break;
            case EYTechTreeNodePerkType.PassiveKMarkGenHour:
                techTreeBonuses.KmarksRate += (int)nodeLevelInfo.PerkAmount;
                break;
            case EYTechTreeNodePerkType.IncreasePassiveKMarkGenCap:
                techTreeBonuses.KmarksCap += (int)nodeLevelInfo.PerkAmount;
                break;
            case EYTechTreeNodePerkType.PassiveAurumGenDay:
                techTreeBonuses.AurumRate = (float)Math.Round(nodeLevelInfo.PerkAmount + techTreeBonuses.AurumRate, 2);
                break;
            case EYTechTreeNodePerkType.IncreasePassiveAurumGenCap:
                techTreeBonuses.AurumCap += (int)nodeLevelInfo.PerkAmount;
                break;
            case EYTechTreeNodePerkType.ReduceUpgradingTimePerc:
                techTreeBonuses.UpgradeSpeedFactor = (float)Math.Round(nodeLevelInfo.PerkAmount + techTreeBonuses.UpgradeSpeedFactor, 2);
                break;
            default:
                break;
        }

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["Balance"] = JsonSerializer.Serialize(balance),
                ["TechTreeNodeData"] = JsonSerializer.Serialize(userTechTree),
                ["CharacterTechTreeBonuses"] = JsonSerializer.Serialize(techTreeBonuses),
            }
        );

        return new FYSkipTechTreeNodeUpgradeClientResult
        {
            UserID = userId,
            Error = "",
            UpgradedNode = node,
            RemainingTimeInSeconds = 0,
            ChangedItems = [],
            DeletedItemsIds = [],
            ChangedCurrencies = changedCurrency,
        };
    }
}