using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;
public class SquadMemberStartingDeployFlowRequest
{
    [JsonPropertyName("squadId")]
    public string SquadID { get; set; }
}

public class SquadMemberStartingDeployFlowResponse
{
    // TODO: Unknown structure
}

[CloudScriptFunction("SquadMemberStartingDeployFlow")]
public class SquadMemberStartingDeployFlowFunction : ICloudScriptFunction<SquadMemberStartingDeployFlowRequest, SquadMemberStartingDeployFlowResponse>
{
    private readonly ILogger<SquadMemberStartingDeployFlowFunction> _logger;

    public SquadMemberStartingDeployFlowFunction(ILogger<SquadMemberStartingDeployFlowFunction> logger)
    {
        _logger = logger;
    }

    public async Task<SquadMemberStartingDeployFlowResponse> ExecuteAsync(SquadMemberStartingDeployFlowRequest request)
    {
        return new SquadMemberStartingDeployFlowResponse
        {};
    }
}
