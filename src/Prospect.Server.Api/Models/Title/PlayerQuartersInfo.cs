using System.Text.Json.Serialization;

public class TitleDataPlayerQuartersInfo {
    [JsonPropertyName("Name")]
    public string Name { get; set; }
    [JsonPropertyName("NodeUpgradesRequired")]
    public int NodeUpgradesRequired { get; set; }
    [JsonPropertyName("UpgradeCosts")]
    public TitleDataPlayerQuartersUpgradeCosts[] UpgradeCosts { get; set; }
    [JsonPropertyName("UpgradeSeconds")]
    public int UpgradeSeconds { get; set; }
    [JsonPropertyName("InitialRushCosts")]
    public int InitialRushCosts { get; set; }
    [JsonPropertyName("OptionalRushCosts")]
    public int OptionalRushCosts { get; set; }
}

public class TitleDataPlayerQuartersUpgradeCosts {
    [JsonPropertyName("Currency")]
    public string Currency { get; set; }
    [JsonPropertyName("Amount")]
    public int Amount { get; set; }
}
