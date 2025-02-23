using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class CheckAndPerformUserRollbackIfNeededRequest
{
    [JsonPropertyName("tryBypass")]
    public bool TryBypass { get; set; }
    [JsonPropertyName("loginNonce")]
    public string LoginNonce { get; set; }
}

public class CheckAndPerformUserRollbackIfNeededResponse
{
    [JsonPropertyName("result")]
    public int Result { get; set; }
}

[CloudScriptFunction("CheckAndPerformUserRollbackIfNeeded")]
public class CheckAndPerformUserRollbackIfNeededFunction : ICloudScriptFunction<CheckAndPerformUserRollbackIfNeededRequest, CheckAndPerformUserRollbackIfNeededResponse>
{
    private readonly ILogger<CheckAndPerformUserRollbackIfNeededFunction> _logger;

    public CheckAndPerformUserRollbackIfNeededFunction(ILogger<CheckAndPerformUserRollbackIfNeededFunction> logger)
    {
        _logger = logger;
    }

    public async Task<CheckAndPerformUserRollbackIfNeededResponse> ExecuteAsync(CheckAndPerformUserRollbackIfNeededRequest request)
    {
        _logger.LogInformation("Processing user rollback if needed");

        return new CheckAndPerformUserRollbackIfNeededResponse
        {
            Result = 1,
        };
    }
}
