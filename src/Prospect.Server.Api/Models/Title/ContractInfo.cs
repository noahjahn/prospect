using System.Text.Json.Serialization;

public class TitleDataContractInfo {
    [JsonPropertyName("Name")]
    public string Name { get; set; }
    [JsonPropertyName("Faction")]
    public string Faction { get; set; }
    [JsonPropertyName("ReputationIncrease")]
    public int ReputationIncrease { get; set; }
    [JsonPropertyName("Rewards")]
    public TitleDataContractInfoReward[] Rewards { get; set; }
    [JsonPropertyName("Objectives")]
    public TitleDataContractInfoObjective[] Objectives { get; set; }
    [JsonPropertyName("UnlockData")]
    public TitleDataContractInfoUnlockData UnlockData { get; set; }
    [JsonPropertyName("IsMainContract")]
    public bool IsMainContract { get; set; }
}

public class TitleDataContractInfoReward {
    [JsonPropertyName("ItemID")]
    public string ItemID { get; set; }
    [JsonPropertyName("Amount")]
    public int Amount { get; set; }
}

public class TitleDataContractInfoObjective {
    [JsonPropertyName("Type")]
    public int Type { get; set; }
    [JsonPropertyName("MaxProgress")]
    public int MaxProgress { get; set; }
    [JsonPropertyName("ItemToOwn")]
    public string ItemToOwn { get; set; }
}

public class TitleDataContractInfoUnlockData {
    [JsonPropertyName("Level")]
    public int Level { get; set; }
    [JsonPropertyName("Contracts")]
    public string[] Contracts { get; set; }
}