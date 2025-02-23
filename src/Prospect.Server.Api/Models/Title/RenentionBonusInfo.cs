using System.Text.Json.Serialization;

public class TitleDataRetentionBonusInfo {
    [JsonPropertyName("Rewards")]
    public string[] Rewards { get; set; }
    [JsonPropertyName("Active")]
    public bool Active { get; set; }
}
