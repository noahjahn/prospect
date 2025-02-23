using System;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript.Models.Data;

namespace Prospect.Server.Api.Services.CloudScript.Models;

public class FYRetentionBonusRequest
{
    
}

public class FYRetentionBonusResult
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
    [JsonPropertyName("playerData")]
    public FYRetentionProgress PlayerData { get; set; }
};

public class FYRetentionProgress
{
    [JsonPropertyName("daysClaimed")]
    public int DaysClaimed { get; set; }
    [JsonPropertyName("lastClaimTime")]
    public FYTimestamp LastClaimTime { get; set; }
    [JsonPropertyName("claimedAll")]
    public bool ClaimedAll { get; set; }
};