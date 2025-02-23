using System.Text.Json.Serialization;

namespace Prospect.Server.Api.Services.CloudScript.Models;

public class FYEnterMatchmakingResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    [JsonPropertyName("maintenanceMode")] // TODO: Legacy param?
    public bool MaintenanceMode { get; set; }
    [JsonPropertyName("blocker")]
	public int Blocker { get; set; } // EYMatchmakingBlocker
    [JsonPropertyName("isMatchTravel")]
	public bool IsMatchTravel { get; set; }
    [JsonPropertyName("singleplayerStation")]
    public bool SingleplayerStation { get; set; }
    [JsonPropertyName("numAttempts")]
	public int NumAttempts { get; set; }
    [JsonPropertyName("sessionId")]
	public string SessionId { get; set; }
    [JsonPropertyName("errorMessage")]
	public string ErrorMessage { get; set; }
}
