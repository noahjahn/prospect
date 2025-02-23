using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class RequestMigrateSteamAccountFromCB1Request
{
    [JsonPropertyName("steamId")]
    public string SteamID { get; set; }
}

public class RequestMigrateSteamAccountFromCB1Response
{}

[CloudScriptFunction("RequestMigrateSteamAccountFromCB1")]
public class RequestMigrateSteamAccountFromCB1Function : ICloudScriptFunction<RequestMigrateSteamAccountFromCB1Request, RequestMigrateSteamAccountFromCB1Response>
{
    private readonly ILogger<RequestMigrateSteamAccountFromCB1Function> _logger;

    public RequestMigrateSteamAccountFromCB1Function(ILogger<RequestMigrateSteamAccountFromCB1Function> logger)
    {
        _logger = logger;
    }

    public async Task<RequestMigrateSteamAccountFromCB1Response> ExecuteAsync(RequestMigrateSteamAccountFromCB1Request request)
    {
        _logger.LogInformation("Processing CB1 account migration request request for Steam account {SteamID}",
            request.SteamID);

        return new RequestMigrateSteamAccountFromCB1Response
        {};
    }
}
