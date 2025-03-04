using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;

public class RequestUpdateSeasonWipeDataRequest
{
}

public class RequestUpdateSeasonWipeDataResponse
{
    [JsonPropertyName("userId")]
	public string UserId { get; set; }
    [JsonPropertyName("error")]
	public string Error { get; set; }
}

[CloudScriptFunction("RequestUpdateSeasonWipeData")]
public class RequestUpdateSeasonWipeDataFunction : ICloudScriptFunction<RequestUpdateSeasonWipeDataRequest, RequestUpdateSeasonWipeDataResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RequestUpdateSeasonWipeDataFunction> _logger;

    public RequestUpdateSeasonWipeDataFunction(ILogger<RequestUpdateSeasonWipeDataFunction> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RequestUpdateSeasonWipeDataResponse> ExecuteAsync(RequestUpdateSeasonWipeDataRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        return new RequestUpdateSeasonWipeDataResponse
        {
            UserId = userId,
            Error = "",
        };
    }
}
