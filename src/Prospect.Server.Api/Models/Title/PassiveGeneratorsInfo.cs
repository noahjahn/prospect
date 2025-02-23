using System.Text.Json.Serialization;

public class TitleDataPassiveGeneratorsInfo {
    [JsonPropertyName("Name")]
    public string Name { get; set; }
    [JsonPropertyName("GeneratorID")]
    public string GeneratorID { get; set; }
    [JsonPropertyName("BaseGenIntervalMinutes")]
    public int BaseGenIntervalMinutes { get; set; }
    [JsonPropertyName("BaseGenRate")]
    public int BaseGenRate { get; set; }
    [JsonPropertyName("BaseCap")]
    public int BaseCap { get; set; }
}
