using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class GetLoginQueuePositionRequest
{
    [JsonPropertyName("tryBypass")]
    public bool TryBypass { get; set; }
    [JsonPropertyName("loginNonce")]
    public string LoginNonce { get; set; }
    [JsonPropertyName("region")]
    public string Region { get; set; }
}

public class GetLoginQueuePositionResponse
{
    [JsonPropertyName("waitingUserCount")]
    public int WaitingUserCount { get; set; }
}

[CloudScriptFunction("GetLoginQueuePosition")]
public class GetLoginQueuePositionFunction : ICloudScriptFunction<GetLoginQueuePositionRequest, GetLoginQueuePositionResponse>
{
    private readonly ILogger<GetLoginQueuePositionFunction> _logger;

    public GetLoginQueuePositionFunction(ILogger<GetLoginQueuePositionFunction> logger)
    {
        _logger = logger;
    }

    public async Task<GetLoginQueuePositionResponse> ExecuteAsync(GetLoginQueuePositionRequest request)
    {
        _logger.LogInformation("Processing user rollback if needed");

        return new GetLoginQueuePositionResponse
        {
            WaitingUserCount = 0,
        };
    }
}
