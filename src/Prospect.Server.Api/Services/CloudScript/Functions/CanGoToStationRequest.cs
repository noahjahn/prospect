using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class CanGoToStationRequestRequest
{
    [JsonPropertyName("tryBypass")]
    public bool TryBypass { get; set; }
    [JsonPropertyName("loginNonce")]
    public string LoginNonce { get; set; }
    [JsonPropertyName("region")]
    public string Region { get; set; }
}

public class CanGoToStationRequestResponse
{
    [JsonPropertyName("canGoToStation")]
    public bool CanGoToStation { get; set; }
    [JsonPropertyName("delta")]
    public int Delta { get; set; }
    [JsonPropertyName("timeStampUpdate")]
    public int TimeStampUpdate { get; set; }
}

[CloudScriptFunction("CanGoToStationRequest")]
public class CanGoToStationRequestFunction : ICloudScriptFunction<CanGoToStationRequestRequest, CanGoToStationRequestResponse>
{
    private readonly ILogger<CanGoToStationRequestFunction> _logger;

    public CanGoToStationRequestFunction(ILogger<CanGoToStationRequestFunction> logger)
    {
        _logger = logger;
    }

    public async Task<CanGoToStationRequestResponse> ExecuteAsync(CanGoToStationRequestRequest request)
    {
        return new CanGoToStationRequestResponse
        {
            CanGoToStation = true,
            Delta = 0,
            TimeStampUpdate = 0,
        };
    }
}
