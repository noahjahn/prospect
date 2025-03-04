using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Models.Data;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.UserData;
using Prospect.Server.Api.Utils;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYSkipItemCraftingClientRequest {
    [JsonPropertyName("useOptionalCosts")]
	public bool UseOptionalCosts { get; set; }
}

public class FYSkipItemCraftingClientResult {
    [JsonPropertyName("userId")]
	public string UserID { get; set; }
    [JsonPropertyName("error")]
	public string Error { get; set; }
    [JsonPropertyName("craftedItemData")]
	public FYCraftedItemData CraftedItemData { get; set; }
    [JsonPropertyName("changedCurrencies")]
	public FYCurrencyItem[] ChangedCurrencies { get; set; }
}

public class FYCraftedItemData
{
    [JsonPropertyName("itemsGrantedOrUpdated")]
	public List<FYCustomItemInfo> ItemsGrantedOrUpdated { get; set; }
    [JsonPropertyName("BlueprintName")]
	public string BlueprintName { get; set; }
};

[CloudScriptFunction("SkipItemCraftingClient")]
public class SkipItemCraftingClient : ICloudScriptFunction<FYSkipItemCraftingClientRequest, FYSkipItemCraftingClientResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public SkipItemCraftingClient(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYSkipItemCraftingClientResult> ExecuteAsync(FYSkipItemCraftingClientRequest request)
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

        var balance = JsonSerializer.Deserialize<PlayerBalance>(userData["Balance"].Value);
        var inventory = JsonSerializer.Deserialize<List<FYCustomItemInfo>>(userData["Inventory"].Value);
        var craftingTimer = JsonSerializer.Deserialize<FYItemCurrentlyBeingCrafted>(userData["CraftingTimer__2022_05_12"].Value);

        var blueprints = JsonSerializer.Deserialize<Dictionary<string, TitleDataBlueprintInfo>>(titleData["Blueprints"]);

        var blueprintName = craftingTimer.ItemID;
        var blueprintData = blueprints[blueprintName];

        // Validate price
        var shopCraftingData = blueprintData.ItemShopsCraftingData["CraftingStation"];
        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var craftEndTime = craftingTimer.UtcTimestampWhenCraftingStarted.Seconds + shopCraftingData.UpgradeTimeSeconds;
        var craftStartTime = craftingTimer.UtcTimestampWhenCraftingStarted.Seconds;
        FYCurrencyItem[] changedCurrency;
        if (request.UseOptionalCosts) {
            var remaining = MapValue.Map(now, craftStartTime, craftEndTime, shopCraftingData.SkipOptionalCraftingMaxCost, 1);
            if (balance.SoftCurrency < remaining) {
                return new FYSkipItemCraftingClientResult {
                    UserID = userId,
                    Error = "Insufficient balance",
                };
            }
            balance.SoftCurrency -= remaining;
            changedCurrency = [new FYCurrencyItem { CurrencyName = "SoftCurrency", Amount = balance.SoftCurrency }];
        } else {
            var remaining = MapValue.Map(now, craftStartTime, craftEndTime, shopCraftingData.SkipCraftingMaxCost, 1);
            if (balance.HardCurrency < remaining) {
                return new FYSkipItemCraftingClientResult {
                    UserID = userId,
                    Error = "Insufficient balance",
                };
            }
            balance.HardCurrency -= remaining;
            changedCurrency = [new FYCurrencyItem { CurrencyName = "Aurum", Amount = balance.HardCurrency }];
        }

        List<FYCustomItemInfo> itemsGrantedOrUpdated = [];
        var remainingAmount = blueprintData.AmountPerPurchase;
        // Populate incomplete stacks first
        foreach (var item in inventory) {
            if (item.BaseItemId != blueprintName || item.Amount >= blueprintData.MaxAmountPerStack) {
                continue;
            }
            var remainingSpace = blueprintData.MaxAmountPerStack - item.Amount;
            var amountToAdd = Math.Min(remainingAmount, remainingSpace);
            item.Amount += amountToAdd;
            remainingAmount -= amountToAdd;
            itemsGrantedOrUpdated.Add(item);
            if (remainingAmount == 0) {
                break;
            }
        }

        while (remainingAmount > 0) {
            var itemStackAmount = Math.Min(blueprintData.MaxAmountPerStack, remainingAmount);
            var craftedItem = new FYCustomItemInfo {
                ItemId = Guid.NewGuid().ToString(),
                Amount = itemStackAmount,
                BaseItemId = blueprintName,
                Durability = blueprintData.DurabilityMax,
                Insurance = "",
                InsuranceOwnerPlayfabId = "",
                ModData = new FYModItems { M = [] },
                Origin = new FYItemOriginBackend { G = "", P = "", T = "" },
                InsuredAttachmentId = "",
                PrimaryVanityId = 0,
                SecondaryVanityId = 0,
                RolledPerks = [],
            };
            inventory.Add(craftedItem);
            itemsGrantedOrUpdated.Add(craftedItem);
            remainingAmount -= itemStackAmount;
        }

        craftingTimer.ItemID = "";
        craftingTimer.UtcTimestampWhenCraftingStarted.Seconds = 0;

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["Balance"] = JsonSerializer.Serialize(balance),
                ["Inventory"] = JsonSerializer.Serialize(inventory),
                ["CraftingTimer__2022_05_12"] = JsonSerializer.Serialize(craftingTimer),
            }
        );

        return new FYSkipItemCraftingClientResult
        {
            UserID = userId,
            Error = "",
            CraftedItemData = new FYCraftedItemData {
                BlueprintName = blueprintName,
                ItemsGrantedOrUpdated = itemsGrantedOrUpdated,
            },
            ChangedCurrencies = changedCurrency,
        };
    }
}