using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;
using Prospect.Server.Api.Services.UserData;
using Prospect.Server.Api.Utils;

public class SellItemsClientRequest
{
    [JsonPropertyName("ids")]
    public string[] IDs { get; set; }
    [JsonPropertyName("factionId")]
    public string FactionID { get; set; }
    [JsonPropertyName("inventoryUpdateData")]
    public SellItemsClientInventoryData InventoryUpdateData { get; set; }
}

public class SellItemsClientInventoryData {
    [JsonPropertyName("itemsToAdd")]
    public FYCustomItemInfo[] ItemsToAdd { get; set; }
    [JsonPropertyName("itemsToUpdateAmount")]
    public FYCustomItemInfo[] ItemsToUpdateAmount { get; set; }
    [JsonPropertyName("itemsToRemove")]
    public HashSet<string> ItemsToRemove { get; set; }
}

public class SellItemsClientResponse
{
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
    [JsonPropertyName("scrappedItemIds")]
    public HashSet<string> ScrappedItemIDs { get; set; }
    [JsonPropertyName("changedItems")]
    public FYCustomItemInfo[] ChangedItems { get; set; }
    [JsonPropertyName("changedCurrencies")]
    public FYCurrencyItem[] ChangedCurrencies { get; set; }
    [JsonPropertyName("playerFactionProgressionData")]
    public FYPlayerFactionProgressData PlayerFactionProgressionData { get; set; }
}

public class FYPlayerFactionProgressData {
    [JsonPropertyName("factionId")]
    public string FactionID { get; set; }
    [JsonPropertyName("currentProgression")]
    public int CurrentProgression { get; set; }
}

[CloudScriptFunction("SellItemsClient")]
public class SellItemsClientFunction : ICloudScriptFunction<SellItemsClientRequest, SellItemsClientResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;
    private readonly ILogger<SellItemsClientFunction> _logger;

    public SellItemsClientFunction(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService, ILogger<SellItemsClientFunction> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
        _logger = logger;
    }

    public async Task<SellItemsClientResponse> ExecuteAsync(SellItemsClientRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var progressionFactionKey = "FactionProgression" + request.FactionID;

        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"Inventory", "Balance", progressionFactionKey});
        var inventory = JsonSerializer.Deserialize<List<FYCustomItemInfo>>(userData["Inventory"].Value);
        var balanceData = JsonSerializer.Deserialize<Dictionary<string, int>>(userData["Balance"].Value);
        var factionProgression = JsonSerializer.Deserialize<int>(userData[progressionFactionKey].Value);
        var blueprintsData = _titleDataService.Find(new List<string>{"Blueprints"});
        var blueprints = JsonSerializer.Deserialize<Dictionary<string, TitleDataBlueprintInfo>>(blueprintsData["Blueprints"]);

        var playerBalance = balanceData["SC"];
        HashSet<string> scrappedItemIds = [];
        foreach (var itemId in request.IDs) {
            var inventoryItemIdx = inventory.FindIndex(i => i.ItemId == itemId);
            if (inventoryItemIdx == -1) {
                continue;
            }
            var inventoryItem = inventory[inventoryItemIdx];
            var blueprintData = blueprints[inventoryItem.BaseItemId];
            // TODO: Probably better to decide based on item kind instead
            if (inventoryItem.Durability == -1) {
                factionProgression += blueprintData.OverrideScrappingReputation * inventoryItem.Amount;
                playerBalance += blueprintData.OverrideScrappingReturns * inventoryItem.Amount;
            } else {
                // Items that have durability are not stackable so it's always 1 item
                factionProgression += MapValue.Map(
                    inventoryItem.Durability,
                    blueprintData.DurabilityMax, 0,
                    blueprintData.OverrideScrappingReputation, (int)(blueprintData.OverrideScrappingReputation * blueprintData.DurabilityBrokenScrappingReturnModifier)
                );
                playerBalance += MapValue.Map(
                    inventoryItem.Durability,
                    blueprintData.DurabilityMax, 0,
                    blueprintData.OverrideScrappingReturns, (int)(blueprintData.OverrideScrappingReturns * blueprintData.DurabilityBrokenScrappingReturnModifier)
                );
            }

            scrappedItemIds.Add(itemId);
        }

        // TODO: Optimize
        var newInventory = new List<FYCustomItemInfo>(inventory.Count);
        foreach (var item in inventory) {
            if (!scrappedItemIds.Contains(item.ItemId)) {
                newInventory.Add(item);
            }
        }

        balanceData["SC"] = playerBalance;

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["Inventory"] = JsonSerializer.Serialize(newInventory),
                ["Balance"] = JsonSerializer.Serialize(balanceData),
                [progressionFactionKey] = JsonSerializer.Serialize(factionProgression),
            }
        );

        return new SellItemsClientResponse
        {
            UserID = userId,
            Error = "",
            ChangedCurrencies = [new FYCurrencyItem {
                CurrencyName = "SoftCurrency",
                Amount = playerBalance,
            }],
            PlayerFactionProgressionData = new FYPlayerFactionProgressData {
                FactionID = request.FactionID,
                CurrentProgression = factionProgression,
            },
            ScrappedItemIDs = scrappedItemIds,
            ChangedItems = [], // TODO: Changed items from InventoryUpdateData
        };
    }
}
