using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Models.Data;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;
using Prospect.Server.Api.Services.UserData;

public class PurchaseWeaponShopItemRequest
{
    [JsonPropertyName("blueprintName")]
    public string BlueprintName { get; set; }
    [JsonPropertyName("blueprintRarity")]
    public int BlueprintRarity { get; set; }
    [JsonPropertyName("purchaseAmount")]
    public int PurchaseAmount { get; set; }
    [JsonPropertyName("baseItemId")]
    public string BaseItemID { get; set; }
    [JsonPropertyName("shopItemBelongsTo")]
    public string ShopItemBelongsTo { get; set; }
}

public class PurchaseWeaponShopItemResponse
{
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("itemRarity")]
    public int ItemRarity { get; set; }
    [JsonPropertyName("changedCurrencies")]
    public FYCurrencyItem[] ChangedCurrencies { get; set; }
    [JsonPropertyName("itemsGrantedOrUpdated")]
    public List<FYCustomItemInfo> ItemsGrantedOrUpdated { get; set; }
    [JsonPropertyName("deletedItemsIds")]
    public HashSet<string> DeletedItemsIds { get; set; }
    [JsonPropertyName("purchaseAmount")]
    public int PurchaseAmount { get; set; }
    [JsonPropertyName("blueprintName")]
    public string BlueprintName { get; set; }
    [JsonPropertyName("shopItemBelongsTo")]
    public string ShopItemBelongsTo { get; set; }
}

public class FYCurrencyItem {
    [JsonPropertyName("currencyName")]
    public string CurrencyName { get; set; }
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
}

public class FYModItems {
    [JsonPropertyName("m")]
	public int[] M { get; set; } // mods
};

// FYItemOriginData
public class FYItemOriginBackend {
    [JsonPropertyName("t")]
	public string T { get; set; } // type
    [JsonPropertyName("p")]
	public string P { get; set; } // playfabid
    [JsonPropertyName("g")]
	public string G { get; set; } // guid
};

// FYRolledPerkEntry
public class FYRolledPerkBackend {
    [JsonPropertyName("i")]
	public int I { get; set; } // short perk id
    [JsonPropertyName("r")]
	public float R { get; set; } // rolled range value
};

public class FYCustomItemInfo {
    [JsonPropertyName("itemId")]
	public string ItemId { get; set; }
    [JsonPropertyName("baseItemId")]
	public string BaseItemId { get; set; }
    [JsonPropertyName("primaryVanityId")] // Item skin
	public int PrimaryVanityId { get; set; }
    [JsonPropertyName("secondaryVanityId")] // Item charm
	public int SecondaryVanityId { get; set; }
    [JsonPropertyName("amount")]
	public int Amount { get; set; }
    [JsonPropertyName("durability")]
	public int Durability { get; set; }
    [JsonPropertyName("modData")]
	public FYModItems ModData { get; set; }
    [JsonPropertyName("rolledPerks")]
	public FYRolledPerkBackend[] RolledPerks { get; set; }
    [JsonPropertyName("insurance")]
	public string Insurance { get; set; }
    [JsonPropertyName("insuranceOwnerPlayfabId")]
	public string InsuranceOwnerPlayfabId { get; set; }
    [JsonPropertyName("insuredAttachmentId")]
	public string InsuredAttachmentId { get; set; }
    [JsonPropertyName("origin")]
	public FYItemOriginBackend Origin { get; set; }
};

public class CustomVanityItem {
    [JsonPropertyName("baseItemId")]
	public string BaseItemId { get; set; }
    [JsonPropertyName("origin")]
	public FYItemOriginBackend Origin { get; set; }
};

[CloudScriptFunction("PurchaseWeaponShopItem")]
public class PurchaseWeaponShopItemFunction : ICloudScriptFunction<PurchaseWeaponShopItemRequest, PurchaseWeaponShopItemResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;
    private readonly ILogger<PurchaseWeaponShopItemFunction> _logger;

    public PurchaseWeaponShopItemFunction(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService, ILogger<PurchaseWeaponShopItemFunction> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
        _logger = logger;
    }

    public async Task<PurchaseWeaponShopItemResponse> ExecuteAsync(PurchaseWeaponShopItemRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        if (request.PurchaseAmount <= 0) {
            return new PurchaseWeaponShopItemResponse
            {
                UserID = userId,
                Error = "Invalid purchase amount",
            };
        }
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"Inventory", "Balance"});
        var inventory = JsonSerializer.Deserialize<List<FYCustomItemInfo>>(userData["Inventory"].Value);
        var balance = JsonSerializer.Deserialize<PlayerBalance>(userData["Balance"].Value);
        var blueprintsData = _titleDataService.Find(new List<string>{"Blueprints"});
        var blueprints = JsonSerializer.Deserialize<Dictionary<string, TitleDataBlueprintInfo>>(blueprintsData["Blueprints"]);
        if (!blueprints.ContainsKey(request.BaseItemID)) {
            _logger.LogError("Failed to find item ID {ItemID}", request.BaseItemID);
            return new PurchaseWeaponShopItemResponse
            {
                UserID = userId,
                Error = "Invalid base item ID",
            };
        }

        var blueprintData = blueprints[request.BaseItemID];

        // TODO: Contract unlock criteria

        // Validate price
        var shopCraftingData = blueprintData.ItemShopsCraftingData[request.ShopItemBelongsTo];
        // TODO: Refactor
        List<FYCustomItemInfo> grantedOrUpdated = [];
        HashSet<string> deletedItemsIds = [];
        List<FYCurrencyItem> changedCurrencies = [];
        foreach (var ingredient in shopCraftingData.ItemRecipeIngredients) {
            int remaining = ingredient.Amount;
            if (ingredient.Currency == "SoftCurrency") {
                var buyCost = request.PurchaseAmount * ingredient.Amount;
                remaining -= buyCost;
                balance.SoftCurrency -= buyCost;
                changedCurrencies.Add(new FYCurrencyItem { CurrencyName = "SoftCurrency", Amount = balance.SoftCurrency });
            } else if (ingredient.Currency == "Aurum") {
                var buyCost = request.PurchaseAmount * ingredient.Amount;
                remaining -= buyCost;
                balance.HardCurrency -= buyCost;
                changedCurrencies.Add(new FYCurrencyItem { CurrencyName = "Aurum", Amount = balance.HardCurrency });
            } else {
                foreach (var item in inventory) {
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
                        grantedOrUpdated.Add(item);
                    }
                    if (remaining <= 0) {
                        break;
                    }
                }
            }
            if (remaining > 0) {
                return new PurchaseWeaponShopItemResponse
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

        // TODO: Inventory limit check
        // TODO: Refactor into a helper function
        var remainingAmount = request.PurchaseAmount * blueprintData.AmountPerPurchase;

        // Populate incomplete stacks first
        foreach (var item in newInventory) {
            if (item.BaseItemId != request.BaseItemID || item.Amount >= blueprintData.MaxAmountPerStack) {
                continue;
            }
            var remainingSpace = blueprintData.MaxAmountPerStack - item.Amount;
            var amountToAdd = Math.Min(remainingAmount, remainingSpace);
            item.Amount += amountToAdd;
            remainingAmount -= amountToAdd;
            grantedOrUpdated.Add(item);
            if (remainingAmount == 0) {
                break;
            }
        }

        // Then create new items
        while (remainingAmount > 0) {
            var itemStackAmount = Math.Min(blueprintData.MaxAmountPerStack, remainingAmount);
            var grantedItem = new FYCustomItemInfo {
                ItemId = Guid.NewGuid().ToString(),
                Amount = itemStackAmount,
                BaseItemId = request.BaseItemID,
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
            newInventory.Add(grantedItem);
            grantedOrUpdated.Add(grantedItem);
            remainingAmount -= itemStackAmount;
        }
        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["Inventory"] = JsonSerializer.Serialize(newInventory),
                ["Balance"] = JsonSerializer.Serialize(balance),
            }
        );

        return new PurchaseWeaponShopItemResponse
        {
            UserID = userId,
            Error = "",
            BlueprintName = request.BlueprintName,
            ItemRarity = request.BlueprintRarity,
            PurchaseAmount = request.PurchaseAmount,
            ChangedCurrencies = changedCurrencies.ToArray(),
            DeletedItemsIds = deletedItemsIds,
            ItemsGrantedOrUpdated = grantedOrUpdated,
            ShopItemBelongsTo = request.ShopItemBelongsTo
        };
    }
}
