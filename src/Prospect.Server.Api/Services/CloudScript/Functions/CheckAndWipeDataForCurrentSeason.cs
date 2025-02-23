using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class CheckAndWipeDataForCurrentSeasonRequest
{
    [JsonPropertyName("tryBypass")]
    public bool TryBypass { get; set; }
    [JsonPropertyName("loginNonce")]
    public string LoginNonce { get; set; }
}

public class CheckAndWipeDataForCurrentSeasonResponse
{
    [JsonPropertyName("result")]
    public int Result { get; set; }
}

[CloudScriptFunction("CheckAndWipeDataForCurrentSeason")]
public class CheckAndWipeDataForCurrentSeasonFunction : ICloudScriptFunction<CheckAndWipeDataForCurrentSeasonRequest, CheckAndWipeDataForCurrentSeasonResponse>
{
    private readonly ILogger<CheckAndWipeDataForCurrentSeasonFunction> _logger;

    public CheckAndWipeDataForCurrentSeasonFunction(ILogger<CheckAndWipeDataForCurrentSeasonFunction> logger)
    {
        _logger = logger;
    }

    public async Task<CheckAndWipeDataForCurrentSeasonResponse> ExecuteAsync(CheckAndWipeDataForCurrentSeasonRequest request)
    {
        _logger.LogInformation("Processing check data wipe for current season");

        return new CheckAndWipeDataForCurrentSeasonResponse
        {
            Result = 1,
        };
    }
}
