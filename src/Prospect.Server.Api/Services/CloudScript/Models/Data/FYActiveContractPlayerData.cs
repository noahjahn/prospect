using System.Text.Json.Serialization;

namespace Prospect.Server.Api.Services.CloudScript.Models.Data;

public class FYActiveContractPlayerData
{
    [JsonPropertyName("contractId")]
    public string ContractID { get; set; } = null!;

    [JsonPropertyName("progress")]
    public int[] Progress { get; set; } = null!;
}