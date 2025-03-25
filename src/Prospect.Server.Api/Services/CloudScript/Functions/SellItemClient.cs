using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Models.Data;
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
    public List<FYCustomItemInfo> ChangedItems { get; set; }
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
        var progressionFactionKey = $"FactionProgression{request.FactionID}";

        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"Inventory", "Balance", progressionFactionKey});
        var inventory = JsonSerializer.Deserialize<List<FYCustomItemInfo>>(userData["Inventory"].Value);
        var balance = JsonSerializer.Deserialize<PlayerBalance>(userData["Balance"].Value);
        var factionProgression = JsonSerializer.Deserialize<int>(userData[progressionFactionKey].Value);

        var blueprintsData = _titleDataService.Find(new List<string>{"Blueprints"});
        var blueprints = JsonSerializer.Deserialize<Dictionary<string, TitleDataBlueprintInfo>>(blueprintsData["Blueprints"]);

        // TODO: Optimize
        // TODO: Check deleted items to see if other stacks/mods were updated correctly
        // Process inventory update before selling since a player may split item stack.
        // This will result in creating a new item and updating amount of existing item.
        // NOTE: Stacking items back doesn't seem to work, at least in single-player station. Bug?
        // var newInventory = new List<FYCustomItemInfo>(inventory.Count);
        // foreach (var item in inventory) {
        //     if (!request.InventoryUpdateData.ItemsToRemove.Contains(item.ItemId)) {
        //         newInventory.Add(item);
        //     }
        // }

        var changedItems = new List<FYCustomItemInfo>();
        foreach (var item in request.InventoryUpdateData.ItemsToUpdateAmount) {
            var inventoryItem = inventory.Find(i => i.ItemId == item.ItemId);
            if (inventoryItem == null) {
                continue;
            }
            inventoryItem.Amount = item.Amount;
            // NOTE: Vanity and mod data cannot be managed in sell menu
            changedItems.Add(item);
        }

        foreach (var item in request.InventoryUpdateData.ItemsToAdd) {
            inventory.Add(item);
        }

        // Then process selling. The game client provides item IDs for newly added items.
        HashSet<string> scrappedItemIds = [];
        foreach (var itemId in request.IDs) {
            var inventoryItem = inventory.Find(i => i.ItemId == itemId);
            if (inventoryItem == null) {
                continue;
            }
            var blueprintData = blueprints[inventoryItem.BaseItemId];
            // NOTE: Ammo and consumables have reverse price mapping.
            if (blueprintData.Kind == "Ammo" || blueprintData.Kind == "Ability") {
                factionProgression += (int)((float)blueprintData.OverrideScrappingReputation / blueprintData.MaxAmountPerStack * inventoryItem.Amount);
                balance.SoftCurrency += (int)((float)blueprintData.OverrideScrappingReturns / blueprintData.MaxAmountPerStack * inventoryItem.Amount);
            } else if (blueprintData.Kind == "Armor") {
                factionProgression += (int)Math.Max(
                    (float)inventoryItem.Durability / blueprintData.DurabilityMax * blueprintData.OverrideScrappingReputation,
                    blueprintData.OverrideScrappingReputation * blueprintData.DurabilityBrokenScrappingReturnModifier
                );
                balance.SoftCurrency += (int)Math.Max(
                    (float)inventoryItem.Durability / blueprintData.DurabilityMax * blueprintData.OverrideScrappingReturns,
                    blueprintData.OverrideScrappingReturns * blueprintData.DurabilityBrokenScrappingReturnModifier
                );
            } else {
                factionProgression += blueprintData.OverrideScrappingReputation  * inventoryItem.Amount;
                balance.SoftCurrency += blueprintData.OverrideScrappingReturns * inventoryItem.Amount;
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

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["Inventory"] = JsonSerializer.Serialize(newInventory),
                ["Balance"] = JsonSerializer.Serialize(balance),
                [progressionFactionKey] = JsonSerializer.Serialize(factionProgression),
            }
        );

        return new SellItemsClientResponse
        {
            UserID = userId,
            Error = "",
            ChangedCurrencies = [new FYCurrencyItem {
                CurrencyName = "SoftCurrency",
                Amount = balance.SoftCurrency,
            }],
            PlayerFactionProgressionData = new FYPlayerFactionProgressData {
                FactionID = request.FactionID,
                CurrentProgression = factionProgression,
            },
            ScrappedItemIDs = scrappedItemIds,
            ChangedItems = changedItems,
        };
    }
}
