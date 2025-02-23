using System.Text.Json.Serialization;

public class TitleDataVanityInfo {
    [JsonPropertyName("Name")]
    public string Name { get; set; }
    [JsonPropertyName("StoreData")]
    public TitleDataVanityInfoStoreData StoreData { get; set; }
}

public class TitleDataVanityInfoStoreData {
    [JsonPropertyName("Amount")]
    public int Amount { get; set; }
}
