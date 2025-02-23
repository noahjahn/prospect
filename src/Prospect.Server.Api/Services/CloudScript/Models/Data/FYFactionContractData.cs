using System.Text.Json.Serialization;

namespace Prospect.Server.Api.Services.CloudScript.Models.Data;

public class FYFactionContractData
{
    [JsonPropertyName("contractId")]
    public string ContractId { get; set; } = null!;
}