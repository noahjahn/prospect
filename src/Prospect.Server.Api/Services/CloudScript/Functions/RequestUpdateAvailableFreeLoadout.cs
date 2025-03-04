using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;

public class RequestUpdateAvailableFreeLoadoutRequest
{
}

public class RequestUpdateAvailableFreeLoadoutResponse
{
    [JsonPropertyName("userId")]
	public string UserId { get; set; }
    [JsonPropertyName("error")]
	public string Error { get; set; }
    [JsonPropertyName("newRandomSeed")]
	public int NewRandomSeed { get; set; }
}

[CloudScriptFunction("RequestUpdateAvailableFreeLoadout")]
public class RequestUpdateAvailableFreeLoadoutFunction : ICloudScriptFunction<RequestUpdateAvailableFreeLoadoutRequest, RequestUpdateAvailableFreeLoadoutResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RequestUpdateAvailableFreeLoadoutFunction> _logger;

    public RequestUpdateAvailableFreeLoadoutFunction(ILogger<RequestUpdateAvailableFreeLoadoutFunction> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RequestUpdateAvailableFreeLoadoutResponse> ExecuteAsync(RequestUpdateAvailableFreeLoadoutRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        return new RequestUpdateAvailableFreeLoadoutResponse
        {
            UserId = userId,
            Error = "",
            NewRandomSeed = 0,
        };
    }
}
