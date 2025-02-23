using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class RegisterUserMacAddressRequestRequest
{
    [JsonPropertyName("macAddress")]
    public string MacAddress { get; set; }
}

public class RegisterUserMacAddressRequestResponse
{
    [JsonPropertyName("result")]
    public bool Result { get; set; }
}

[CloudScriptFunction("RegisterUserMacAddressRequest")]
public class RegisterUserMacAddressRequestFunction : ICloudScriptFunction<RegisterUserMacAddressRequestRequest, RegisterUserMacAddressRequestResponse>
{
    private readonly ILogger<RegisterUserMacAddressRequestFunction> _logger;

    public RegisterUserMacAddressRequestFunction(ILogger<RegisterUserMacAddressRequestFunction> logger)
    {
        _logger = logger;
    }

    public async Task<RegisterUserMacAddressRequestResponse> ExecuteAsync(RegisterUserMacAddressRequestRequest request)
    {
        return new RegisterUserMacAddressRequestResponse
        {
            Result = true,
        };
    }
}
