using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;

public class RequestCanReconnectRequest
{
    // Empty request
}

public class RequestCanReconnectResponse
{
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    //[JsonPropertyName("error")]
    //public int Error { get; set; }
    //[JsonPropertyName("changedItems")]
    //public int changedItems { get; set; }
    //[JsonPropertyName("sessionId")]
    //public int SessionID { get; set; }
    //[JsonPropertyName("info")]
    //public int Info { get; set; }
    //[JsonPropertyName("lastSessionResult")]
    //public int LastSessionResult { get; set; }
    //[JsonPropertyName("lastSessionId")]
    //public int LastSessionID { get; set; }
}

[CloudScriptFunction("RequestCanReconnect")]
public class RequestCanReconnectFunction : ICloudScriptFunction<RequestCanReconnectRequest, RequestCanReconnectResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestCanReconnectFunction(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RequestCanReconnectResponse> ExecuteAsync(RequestCanReconnectRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return new RequestCanReconnectResponse
            {};
        }

        return new RequestCanReconnectResponse
        {
            //Error = 0,
            //Info = 0,
            //LastSessionResult = 0,
            //LastSessionID = 0,
            //SessionID = 0,
            //changedItems = 0,
            UserID = context.User.FindAuthUserId(),
        };
    }
}
