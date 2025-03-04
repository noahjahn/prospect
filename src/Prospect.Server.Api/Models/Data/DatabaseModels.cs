using System.Text.Json.Serialization;

namespace Prospect.Server.Api.Models.Data;

public class PlayerBalance
{
    [JsonPropertyName("AU")]
    public int HardCurrency { get; set; }

    [JsonPropertyName("SC")]
    public int SoftCurrency { get; set; }

    [JsonPropertyName("IN")]
    public int InsuranceTokens { get; set; }
}