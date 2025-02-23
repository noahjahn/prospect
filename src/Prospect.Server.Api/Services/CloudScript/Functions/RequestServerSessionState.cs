using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class FYMatchConnectionData {
    [JsonPropertyName("addr")]
    public string Addr { get; set; }
    [JsonPropertyName("sessionId")]
    public string SessionID { get; set; }
    [JsonPropertyName("serverId")]
    public string ServerID { get; set; }
    [JsonPropertyName("connectSinglePlayer")]
    public bool ConnectSinglePlayer { get; set; }
    [JsonPropertyName("isMatch")]
    public bool IsMatch { get; set; }
}

public class RequestServerSessionStateRequest
{
    [JsonPropertyName("sessionId")]
    public string SessionID { get; set; }
    [JsonPropertyName("amountRequests")]
    public int AmountRequests { get; set; }
}

public class RequestServerSessionStateResponse
{
    [JsonPropertyName("connectionData")]
    public FYMatchConnectionData ConnectionData { get; set; }
    [JsonPropertyName("retryCounter")]
    public int RetryCounter { get; set; }
    [JsonPropertyName("canGoToSession")]
    public bool CanGoToSession { get; set; }
    [JsonPropertyName("shouldCancel")]
    public bool ShouldCancel { get; set; }
}

[CloudScriptFunction("RequestServerSessionState")]
public class RequestServerSessionStateFunction : ICloudScriptFunction<RequestServerSessionStateRequest, RequestServerSessionStateResponse>
{
    private readonly ILogger<RequestServerSessionStateFunction> _logger;

    public RequestServerSessionStateFunction(ILogger<RequestServerSessionStateFunction> logger)
    {
        _logger = logger;
    }

    public async Task<RequestServerSessionStateResponse> ExecuteAsync(RequestServerSessionStateRequest request)
    {
        _logger.LogInformation("Processing server session state");

        return new RequestServerSessionStateResponse
        {
            CanGoToSession = true,
            ConnectionData = new FYMatchConnectionData {
                Addr = request.SessionID, // FIXME: This is for offline use case only!,
                ConnectSinglePlayer = false,
                IsMatch = true,
                ServerID = "testserver",
                SessionID = request.SessionID,
            },
            ShouldCancel = false,
            RetryCounter = 1,
        };
    }
}
