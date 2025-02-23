using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class ClientsideFriendsImportRequest
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; }
    [JsonPropertyName("userIds")]
    public string[] UserIDs { get; set; }
}

public class ClientsideFriendsImportResponse
{
    // TODO: Unknown structure
}

[CloudScriptFunction("ClientsideFriendsImport")]
public class ClientsideFriendsImportFunction : ICloudScriptFunction<ClientsideFriendsImportRequest, ClientsideFriendsImportResponse>
{
    private readonly ILogger<ClientsideFriendsImportFunction> _logger;

    public ClientsideFriendsImportFunction(ILogger<ClientsideFriendsImportFunction> logger)
    {
        _logger = logger;
    }

    public async Task<ClientsideFriendsImportResponse> ExecuteAsync(ClientsideFriendsImportRequest request)
    {
        _logger.LogInformation("Processing friends import");

        return new ClientsideFriendsImportResponse{};
    }
}
