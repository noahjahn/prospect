using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript.Models.Data;

public class FYTechTreeNodeStatus {
    [JsonPropertyName("nodeId")]
	public string NodeID { get; set; }
    [JsonPropertyName("level")]
	public int Level { get; set; }
    [JsonPropertyName("upgradeStartedTime")]
	public FYTimestamp UpgradeStartedTime { get; set; }
};