using System.Text.Json.Serialization;

public class TitleDataDailyCrateInfo {
    [JsonPropertyName("Name")]
    public string Name { get; set; }
    [JsonPropertyName("Weight")]
    public float Weight { get; set; }
    [JsonPropertyName("Amount")]
    public int Amount { get; set; }
}
