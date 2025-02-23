using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

// Season 1 request
public class RequestIsPlayerStillOnBattleServerRequest
{
}

public class RequestIsPlayerStillOnBattleServerResponse
{
    [JsonPropertyName("isStillOnBattleServer")]
	public bool IsStillOnBattleServer { get; set; }
	[JsonPropertyName("infoStillOnBattleServer")]
    public string InfoStillOnBattleServer { get; set; }
}

[CloudScriptFunction("RequestIsPlayerStillOnBattleServer")]
public class RequestIsPlayerStillOnBattleServerFunction : ICloudScriptFunction<RequestIsPlayerStillOnBattleServerRequest, RequestIsPlayerStillOnBattleServerResponse>
{
    private readonly ILogger<RequestIsPlayerStillOnBattleServerFunction> _logger;

    public RequestIsPlayerStillOnBattleServerFunction(ILogger<RequestIsPlayerStillOnBattleServerFunction> logger)
    {
        _logger = logger;
    }
    public async Task<RequestIsPlayerStillOnBattleServerResponse> ExecuteAsync(RequestIsPlayerStillOnBattleServerRequest request)
    {
        return new RequestIsPlayerStillOnBattleServerResponse
        {
            IsStillOnBattleServer = false,
            InfoStillOnBattleServer = "",
        };
    }
}
