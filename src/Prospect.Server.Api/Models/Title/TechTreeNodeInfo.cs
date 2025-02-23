using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript.Functions;

public class TitleDataTechTreeNodeInfo {
    [JsonPropertyName("Name")]
    public string Name { get; set; }
    [JsonPropertyName("PlayerQuarterLevelRequired")]
    public int PlayerQuarterLevelRequired { get; set; }
    [JsonPropertyName("NodePerkType")]
    public EYTechTreeNodePerkType NodePerkType { get; set; }
    [JsonPropertyName("PerkLevels")]
    public TitleDataTechTreeNodePerkLevels[] PerkLevels { get; set; }
}

public class TitleDataTechTreeNodePerkLevels {
    [JsonPropertyName("PerkAmount")]
    public float PerkAmount { get; set; }
    [JsonPropertyName("UpgradeCosts")]
    public TitleDataTechTreeNodePerkLevelsUpgradeCosts[] UpgradeCosts { get; set; }
    [JsonPropertyName("UpgradeSeconds")]
    public int UpgradeSeconds { get; set; }
    [JsonPropertyName("InitialRushCosts")]
    public int InitialRushCosts { get; set; }
    [JsonPropertyName("OptionalRushCosts")]
    public int OptionalRushCosts { get; set; }
}

public class TitleDataTechTreeNodePerkLevelsUpgradeCosts {
    [JsonPropertyName("Currency")]
    public string Currency { get; set; }
    [JsonPropertyName("Amount")]
    public int Amount { get; set; }
}

public class TitleDataTechTreeNodeUpgradeDependencies {
    [JsonPropertyName("RelatedDependency")]
    public string RelatedDependency { get; set; }
    [JsonPropertyName("RequiredLevel")]
    public int RequiredLevel { get; set; }
}
