

using System.Text.Json.Serialization;

public class UDataTable {
    [JsonPropertyName("DataTable")]
    public string DataTable { get; set; }
    [JsonPropertyName("RowName")]
    public string RowName { get; set; }
}