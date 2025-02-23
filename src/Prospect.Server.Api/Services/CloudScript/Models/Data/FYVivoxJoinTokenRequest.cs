using System.Text.Json.Serialization;

namespace Prospect.Server.Api.Services.CloudScript.Models.Data;

public class FYVivoxJoinTokenRequest
{
    [JsonPropertyName("userName")]
    public string Username { get; set; }

    [JsonPropertyName("channel")]
    public string Channel { get; set; }

    [JsonPropertyName("hasText")]
    public bool HasText { get; set; }

    [JsonPropertyName("hasAudio")]
    public bool HasAudio { get; set; }

    [JsonPropertyName("channelType")]
    public int ChannelType { get; set; }

    [JsonPropertyName("channelId")]
    public string ChannelID { get; set; }
}