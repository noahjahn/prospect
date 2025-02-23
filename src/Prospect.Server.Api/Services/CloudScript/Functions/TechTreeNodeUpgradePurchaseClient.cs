using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYTechTreeNodeUpgradePurchaseClientRequest {
    // Empty request
}

public class FYTechTreeNodeUpgradePurchaseClientResult {
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

public enum EYTechTreeNodePerkType {
    None,
    IncreaseStashSize,
    IncreaseBagSize,
    PassiveKMarkGenHour,
    PassiveAurumGenDay,
    IncreasePassiveKMarkGenCap,
    IncreasePassiveAurumGenCap,
    DailyCrate,
    ReduceUpgradingTimePerc,
    MAX
}

[CloudScriptFunction("TechTreeNodeUpgradePurchaseClient")]
public class TechTreeNodeUpgradePurchaseClient : ICloudScriptFunction<FYTechTreeNodeUpgradePurchaseClientRequest, FYTechTreeNodeUpgradePurchaseClientResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public TechTreeNodeUpgradePurchaseClient(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYTechTreeNodeUpgradePurchaseClientResult> ExecuteAsync(FYTechTreeNodeUpgradePurchaseClientRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        var userData = await _userDataService.FindAsync(
            userId, userId,
            new List<string>{"TechTreeNodeData", "CharacterTechTreeBonuses"}
        );

        var userTechTree = JsonSerializer.Deserialize<UserTechTreeNodeData>(userData["TechTreeNodeData"].Value);

        var currentNode = userTechTree.NodeInProgress;
        if (currentNode == "") {
            return new FYTechTreeNodeUpgradePurchaseClientResult
            {
                UserID = userId,
                Error = "Nothing is being constructed",
            };
        }

        var titleData = _titleDataService.Find(new List<string>{"TechTreeNodes"});
        var techTreeNodes = JsonSerializer.Deserialize<Dictionary<string, TitleDataTechTreeNodeInfo>>(titleData["TechTreeNodes"]);

        var node = userTechTree.Nodes[currentNode];
        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var nodeInfo = techTreeNodes[currentNode];
        var nodeLevelInfo = techTreeNodes[currentNode].PerkLevels[node.Level];
        var endTime = nodeLevelInfo.UpgradeSeconds + node.UpgradeStartedTime.Seconds;
        if (now < endTime) {
            return new FYTechTreeNodeUpgradePurchaseClientResult
            {
                UserID = userId,
                Error = "Upgrade is in progress",
            };
        }

        // TODO: Refactor
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
                ["TechTreeNodeData"] = JsonSerializer.Serialize(userTechTree),
                ["CharacterTechTreeBonuses"] = JsonSerializer.Serialize(techTreeBonuses),
            }
        );

        return new FYTechTreeNodeUpgradePurchaseClientResult
        {
            UserID = userId,
            Error = "",
            UpgradedNode = node,
            RemainingTimeInSeconds = 0,
            ChangedItems = [],
            DeletedItemsIds = [],
            ChangedCurrencies = [],
        };
    }
}