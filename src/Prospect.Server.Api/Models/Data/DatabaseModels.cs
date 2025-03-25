using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript.Models.Data;

namespace Prospect.Server.Api.Models.Data;

public class PlayerBalance
{
    [JsonPropertyName("AU")]
    public int HardCurrency { get; set; }

    [JsonPropertyName("SC")]
    public int SoftCurrency { get; set; }

    [JsonPropertyName("IN")]
    public int InsuranceCurrency { get; set; }
}

public class JobBoardsData
{
    [JsonPropertyName("lastBoardRefreshTimeUtc")]
    public FYTimestamp LastBoardRefreshTimeUtc { get; set; }

    [JsonPropertyName("boards")]
    public List<FYFactionContractsData> Boards { get; set; }
}