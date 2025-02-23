using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class CheckClientVersionUpToDateRequest
{
    [JsonPropertyName("version")]
    public string Version { get; set; }
    [JsonPropertyName("tryBypass")]
    public bool TryBypass { get; set; }
    [JsonPropertyName("loginNonce")]
    public string LoginNonce { get; set; }
}

public class CheckClientVersionUpToDateResponse
{
    [JsonPropertyName("isClientUpToDate")]
    public bool IsClientUpToDate { get; set; }
}

[CloudScriptFunction("CheckClientVersionUpToDate")]
public class CheckClientVersionUpToDateFunction : ICloudScriptFunction<CheckClientVersionUpToDateRequest, CheckClientVersionUpToDateResponse>
{
    private readonly ILogger<CheckClientVersionUpToDateFunction> _logger;

    public CheckClientVersionUpToDateFunction(ILogger<CheckClientVersionUpToDateFunction> logger)
    {
        _logger = logger;
    }

    public async Task<CheckClientVersionUpToDateResponse> ExecuteAsync(CheckClientVersionUpToDateRequest request)
    {
        _logger.LogInformation("Processing client version check request for client {Version}",
            request.Version);

        // TODO: Validate login
        // if (string.IsNullOrEmpty(request.LoginNonce))
        // {
        //     return new CheckClientVersionUpToDateResponse
        //     {
        //         IsClientUpToDate = false,
        //     };
        // }

        return new CheckClientVersionUpToDateResponse
        {
            IsClientUpToDate = true,
        };
    }
}
