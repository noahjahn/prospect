using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class UserIdentifiers {
    [JsonPropertyName("ip")]
    public string IP {  get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; }

    [JsonPropertyName("sku")]
    public string SKU { get; set; }

    [JsonPropertyName("oS")]
    public string OS { get; set; }

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; }
}

public class SendGamesightEventRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("revenue_currency")]
    public string RevenueCurrency { get; set; }

    [JsonPropertyName("revenue_amount")]
    public int RevenueAmount { get; set; }

    [JsonPropertyName("identifiers")]
    public UserIdentifiers Identifiers { get; set; }
}

public class SendGamesightEventResponse
{
    // TODO: Unknown structure. Possibly empty response
}

[CloudScriptFunction("SendGamesightEvent")]
public class SendGamesightEventFunction : ICloudScriptFunction<SendGamesightEventRequest, SendGamesightEventResponse>
{
    private readonly ILogger<SendGamesightEventFunction> _logger;

    public SendGamesightEventFunction(ILogger<SendGamesightEventFunction> logger)
    {
        _logger = logger;
    }

    public async Task<SendGamesightEventResponse> ExecuteAsync(SendGamesightEventRequest request)
    {
        _logger.LogInformation("Processing Gamesight Event");

        return new SendGamesightEventResponse{};
    }
}
