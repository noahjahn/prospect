using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;
public class SquadMemberSelectedMapRequest
{
    [JsonPropertyName("squadId")]
    public string SquadID { get; set; }
    [JsonPropertyName("selectedMapName")]
    public string SelectedMapName { get; set; }
}

public class SquadMemberSelectedMapResponse
{
    // TODO: Unknown structure
}

[CloudScriptFunction("SquadMemberSelectedMap")]
public class SquadMemberSelectedMapFunction : ICloudScriptFunction<SquadMemberSelectedMapRequest, SquadMemberSelectedMapResponse>
{
    private readonly ILogger<SquadMemberSelectedMapFunction> _logger;

    public SquadMemberSelectedMapFunction(ILogger<SquadMemberSelectedMapFunction> logger)
    {
        _logger = logger;
    }

    public async Task<SquadMemberSelectedMapResponse> ExecuteAsync(SquadMemberSelectedMapRequest request)
    {
        return new SquadMemberSelectedMapResponse
        {};
    }
}
