using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class YOnLoginRequest
{
    [JsonPropertyName("gamePlatform")]
    public string GamePlatform { get; set; }
    [JsonPropertyName("tryBypass")]
    public bool TryBypass { get; set; }
    [JsonPropertyName("loginNonce")]
    public string LoginNonce { get; set; }
    [JsonPropertyName("customTags")]
    public Dictionary<string, string> CustomTags { get; set; } = new();
}

public class OnLoginResponse
{
    // TODO: Unknown structure
}

[CloudScriptFunction("OnLogin")]
public class OnLoginFunction : ICloudScriptFunction<YOnLoginRequest, OnLoginResponse>
{
    private readonly ILogger<OnLoginFunction> _logger;

    public OnLoginFunction(ILogger<OnLoginFunction> logger)
    {
        _logger = logger;
    }

    public async Task<OnLoginResponse> ExecuteAsync(YOnLoginRequest request)
    {
        _logger.LogInformation("Processing login request for platform {Platform} with nonce {Nonce}",
            request.GamePlatform, request.LoginNonce);

        // Example: Validate login
        // if (string.IsNullOrEmpty(request.LoginNonce))
        // {
        //     return new OnLoginResponse
        //     {};
        // }

        // Simulating user authentication logic
        var sessionToken = Guid.NewGuid().ToString();

        return new OnLoginResponse
        {};
    }
}
