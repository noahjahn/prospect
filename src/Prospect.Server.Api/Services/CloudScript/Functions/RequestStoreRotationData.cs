using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;

public class RequestStoreRotationDataRequest
{
    // Empty request
}

public class StoreRotationData
{
    [JsonPropertyName("storeId")]
    public string StoreID { get; set; }
    [JsonPropertyName("expirationData")]
    public DateTime ExpirationData { get; set; }
}

public class RequestStoreRotationDataResponse
{
    [JsonPropertyName("dailyStoreData")]
    public StoreRotationData DailyStoreData { get; set; }
    [JsonPropertyName("weeklyStoreData")]
    public StoreRotationData WeeklyStoreData { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
}

[CloudScriptFunction("RequestStoreRotationData")]
public class RequestStoreRotationDataFunction : ICloudScriptFunction<RequestStoreRotationDataRequest, RequestStoreRotationDataResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestStoreRotationDataFunction(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RequestStoreRotationDataResponse> ExecuteAsync(RequestStoreRotationDataRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }

        // TODO: Get and save store rotation data

        return new RequestStoreRotationDataResponse
        {
            Error = "",
            DailyStoreData = new StoreRotationData {
                StoreID = "DailyShop",
                ExpirationData = DateTime.UtcNow.AddDays(1)
            },
            WeeklyStoreData = new StoreRotationData {
                StoreID = "WeeklyShop",
                ExpirationData = DateTime.UtcNow.AddDays(1)
            }
        };
    }
}
