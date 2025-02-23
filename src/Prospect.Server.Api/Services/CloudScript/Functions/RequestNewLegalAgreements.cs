using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class RequestNewLegalAgreementsRequest
{
    [JsonPropertyName("gamePlatform")]
    public string GamePlatform { get; set; }
    [JsonPropertyName("nDAVersion")]
    public int NDAVersion { get; set; }
    [JsonPropertyName("eULAVersion")]
    public int EULAVersion { get; set; }
    [JsonPropertyName("tryBypass")]
    public bool TryBypass { get; set; }
    [JsonPropertyName("loginNonce")]
    public string LoginNonce { get; set; }
}

public class RequestNewLegalAgreementsResponse
{
    [JsonPropertyName("requiredNDAVersion")]
    public int NDAVersion { get; set; }
    [JsonPropertyName("requiredEULAVersion")]
    public int EULAVersion { get; set; }
    [JsonPropertyName("hasAccepted")]
    public int HasAccepted { get; set; }
}

[CloudScriptFunction("RequestNewLegalAgreements")]
public class RequestNewLegalAgreementsFunction : ICloudScriptFunction<RequestNewLegalAgreementsRequest, RequestNewLegalAgreementsResponse>
{
    private readonly ILogger<RequestNewLegalAgreementsFunction> _logger;

    public RequestNewLegalAgreementsFunction(ILogger<RequestNewLegalAgreementsFunction> logger)
    {
        _logger = logger;
    }

    public async Task<RequestNewLegalAgreementsResponse> ExecuteAsync(RequestNewLegalAgreementsRequest request)
    {
        _logger.LogInformation("Processing request new legal agreements for platform {Platform} with nonce {Nonce}",
            request.GamePlatform, request.LoginNonce);

        return new RequestNewLegalAgreementsResponse
        {
            NDAVersion = request.NDAVersion,
            EULAVersion = request.EULAVersion,
            HasAccepted = 1,
        };
    }
}
