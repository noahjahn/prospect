using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models.Data;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYStartItemCraftingClientRequest {
    [JsonPropertyName("baseItemId")]
	public string BaseItemID { get; set; }
}

public class FYStartItemCraftingClientResult {
    [JsonPropertyName("userId")]
	public string UserID { get; set; }
    [JsonPropertyName("error")]
	public string Error { get; set; }
    [JsonPropertyName("changedCurrencies")]
	public FYCurrencyItem[] ChangedCurrencies { get; set; }
    [JsonPropertyName("changedItems")]
	public List<FYCustomItemInfo> ChangedItems { get; set; }
    [JsonPropertyName("deletedItemsIds")]
	public HashSet<string> DeletedItemsIds { get; set; }
}

public class FYItemCurrentlyBeingCrafted {
    [JsonPropertyName("itemId")]
	public string ItemID { get; set; }
    [JsonPropertyName("utcTimestampWhenCraftingStarted")]
	public FYTimestamp UtcTimestampWhenCraftingStarted { get; set; }
}

[CloudScriptFunction("StartItemCraftingClient")]
public class StartItemCraftingClient : ICloudScriptFunction<FYStartItemCraftingClientRequest, FYStartItemCraftingClientResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public StartItemCraftingClient(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYStartItemCraftingClientResult> ExecuteAsync(FYStartItemCraftingClientRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var titleData = _titleDataService.Find(new List<string>{"Blueprints"});

        var userData = await _userDataService.FindAsync(
            userId, userId,
            new List<string>{"CraftingTimer__2022_05_12", "Balance", "Inventory"}
        );

        var craftingTimer = JsonSerializer.Deserialize<FYItemCurrentlyBeingCrafted>(userData["CraftingTimer__2022_05_12"].Value);
        if (craftingTimer.UtcTimestampWhenCraftingStarted.Seconds != 0) {
            return new FYStartItemCraftingClientResult
            {
                UserID = userId,
                Error = "Crafting is in progress",
            };
        }

        // TODO: Contract unlock criteria

        var balance = JsonSerializer.Deserialize<Dictionary<string, int>>(userData["Balance"].Value);
        var inventory = JsonSerializer.Deserialize<List<FYCustomItemInfo>>(userData["Inventory"].Value);

        var blueprints = JsonSerializer.Deserialize<Dictionary<string, TitleDataBlueprintInfo>>(titleData["Blueprints"]);

        var blueprintData = blueprints[request.BaseItemID];

        // Validate price
        var shopCraftingData = blueprintData.ItemShopsCraftingData["CraftingStation"];
        // TODO: Refactor
        List<FYCustomItemInfo> changedItems = [];
        HashSet<string> deletedItemsIds = [];
        List<FYCurrencyItem> changedCurrencies = [];
        foreach (var ingredient in shopCraftingData.ItemRecipeIngredients) {
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
                return new FYStartItemCraftingClientResult
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

        craftingTimer.ItemID = request.BaseItemID;
        craftingTimer.UtcTimestampWhenCraftingStarted.Seconds = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["Balance"] = JsonSerializer.Serialize(balance),
                ["Inventory"] = JsonSerializer.Serialize(newInventory),
                ["CraftingTimer__2022_05_12"] = JsonSerializer.Serialize(craftingTimer),
            }
        );

        return new FYStartItemCraftingClientResult
        {
            UserID = userId,
            Error = "",
            ChangedItems = changedItems,
            DeletedItemsIds = deletedItemsIds,
            ChangedCurrencies = changedCurrencies.ToArray(),
        };
    }
}