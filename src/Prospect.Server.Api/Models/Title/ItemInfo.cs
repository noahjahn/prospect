using System.Text.Json.Serialization;

public class TitleDataBlueprintInfo {
    [JsonPropertyName("Name")]
    public string Name { get; set; }
    [JsonPropertyName("ItemShopsCraftingData")]
    public Dictionary<string, TitleDataItemShopsCraftingData> ItemShopsCraftingData { get; set; }
    [JsonPropertyName("AmountPerPurchase")]
    public int AmountPerPurchase { get; set; }
    [JsonPropertyName("DurabilityMax")]
    public int DurabilityMax { get; set; }
    [JsonPropertyName("DurabilityBrokenScrappingReturnModifier")]
    public double DurabilityBrokenScrappingReturnModifier { get; set; }
    [JsonPropertyName("RepairCostMaxDurability")]
    public int RepairCostMaxDurability { get; set; }
    [JsonPropertyName("RepairCostBase")]
    public int RepairCostBase { get; set; }
    [JsonPropertyName("RepairCostModifierBroken")]
    public int RepairCostModifierBroken { get; set; }
    [JsonPropertyName("ItemWeight")]
    public double ItemWeight { get; set; }
    [JsonPropertyName("ScrappingReturnDefault")]
    public int ScrappingReturnDefault { get; set; }
    [JsonPropertyName("OverrideScrappingReturns")]
    public int OverrideScrappingReturns { get; set; }
    [JsonPropertyName("OverrideScrappingReputation")]
    public int OverrideScrappingReputation { get; set; }
    [JsonPropertyName("ScrappingFactionProgressionIncrement")]
    public int ScrappingFactionProgressionIncrement { get; set; }
	[JsonPropertyName("UnlockData")]
    public object UnlockData { get; set; }
	[JsonPropertyName("ModID")]
    public int? ModID { get; set; }
	[JsonPropertyName("AlienForgeCatalystToUpgradedItemMap")]
    public object? AlienForgeCatalystToUpgradedItemMap { get; set; }
    [JsonPropertyName("Rarity")]
    public int Rarity { get; set; }
    [JsonPropertyName("Kind")]
    public string Kind { get; set; }
    [JsonPropertyName("MaxAmountPerStack")]
    public int MaxAmountPerStack { get; set; }
}

public class TitleDataItemShopsCraftingData {
    [JsonPropertyName("ItemRecipeIngredients")]
    public List<TitleDataItemRecipeCostType> ItemRecipeIngredients { get; set; }
	[JsonPropertyName("UpgradeTimeMinutes")]
    public int UpgradeTimeMinutes { get; set; }
	[JsonPropertyName("UpgradeTimeSeconds")]
    public int UpgradeTimeSeconds { get; set; }
	[JsonPropertyName("SkipCraftingMaxCost")]
    public int SkipCraftingMaxCost { get; set; }
	[JsonPropertyName("SkipOptionalCraftingMaxCost")]
    public int SkipOptionalCraftingMaxCost { get; set; }
}

public class TitleDataItemRecipeCostType {
    [JsonPropertyName("Item")]
    public string Currency { get; set; }
    [JsonPropertyName("Amount")]
    public int Amount { get; set; }
}

/*
enum class EYItemType : uint8_t
{
	EYItemType__None               = 0,
	EYItemType__Device             = 1,
	EYItemType__Ability            = 2,
	EYItemType__Kit                = 3,
	EYItemType__Consumable         = 4,
	EYItemType__Mod                = 5,
	EYItemType__Blueprint          = 6,
	EYItemType__Material           = 7,
	EYItemType__Miscellaneous      = 8,
	EYItemType__Currency           = 9,
	EYItemType__Vanity             = 10,
	EYItemType__Experience         = 11,
	EYItemType__Lore               = 12,
	EYItemType__Vehicle            = 13,
	EYItemType__Ammo               = 14,
	EYItemType__Upgrade            = 15,
	EYItemType__Collectible        = 16,
	EYItemType__QuestItem          = 17,
	EYItemType__Shield             = 18,
	EYItemType__ProspectorBadge    = 19,
	EYItemType__TechTreeNode       = 20,
	EYItemType__PlayerQuartersLevel = 21,
	EYItemType__PassiveGenerator   = 22,
	EYItemType__Bag                = 23,
	EYItemType__Helmet             = 24,
	EYItemType__Key                = 25,
	EYItemType__MeleeWeapon        = 26,
	EYItemType__All                = 27,
	EYItemType__MAX                = 28
};

enum class EYItemOriginType : uint8_t
{
	EYItemOriginType__Undefined    = 0,
	EYItemOriginType__Contract     = 1,
	EYItemOriginType__Craft        = 2,
	EYItemOriginType__Creature     = 3,
	EYItemOriginType__Debug        = 4,
	EYItemOriginType__FortunaPass  = 5,
	EYItemOriginType__FTUE         = 6,
	EYItemOriginType__Generator    = 7,
	EYItemOriginType__Insurance    = 8,
	EYItemOriginType__MapContainerLoot = 9,
	EYItemOriginType__MapPickupLoot = 10,
	EYItemOriginType__MatchSplitStack = 11,
	EYItemOriginType__MatchUnknown = 12,
	EYItemOriginType__PlayFabDashboard = 13,
	EYItemOriginType__PurchaseICA  = 14,
	EYItemOriginType__PurchaseKorolev = 15,
	EYItemOriginType__PurchaseOsiris = 16,
	EYItemOriginType__PurchaseQuickShop = 17,
	EYItemOriginType__Retention    = 18,
	EYItemOriginType__StationSplitStack = 19,
	EYItemOriginType__TocMigration = 20,
	EYItemOriginType__TwitchDrop   = 21,
	EYItemOriginType__Uplink       = 22,
	EYItemOriginType__WeaponAttachment = 23,
	EYItemOriginType__EYItemOriginType_MAX = 24
};
*/