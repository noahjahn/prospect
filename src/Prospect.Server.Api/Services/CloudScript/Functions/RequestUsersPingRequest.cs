using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class RegionObject
{
    [JsonPropertyName("region")]
    public string Region { get; set; }
    [JsonPropertyName("ping")]
    public int Ping { get; set; }
    [JsonPropertyName("instanceType")]
    public int InstanceType { get; set; }
}

public class RequestUsersPingRequestRequest
{
    [JsonPropertyName("pings")]
    public RegionObject[] Pings { get; set; }
    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; }
}

public class RequestUsersPingRequestResponse
{
    // TODO: Unknown structure
}

[CloudScriptFunction("RequestUsersPingRequest")]
public class RequestUsersPingRequestFunction : ICloudScriptFunction<RequestUsersPingRequestRequest, RequestUsersPingRequestResponse>
{
    private readonly ILogger<RequestUsersPingRequestFunction> _logger;

    public RequestUsersPingRequestFunction(ILogger<RequestUsersPingRequestFunction> logger)
    {
        _logger = logger;
    }

    public async Task<RequestUsersPingRequestResponse> ExecuteAsync(RequestUsersPingRequestRequest request)
    {
        _logger.LogInformation("Processing ping request");

        return new RequestUsersPingRequestResponse
        {
        };
    }
}
