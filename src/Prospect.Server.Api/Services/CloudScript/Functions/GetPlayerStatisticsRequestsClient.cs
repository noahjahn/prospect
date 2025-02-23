using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

// ["DropsTotal","DeathsTotal","DeathsPlayerTotal","DeathsCreaturesTotal","DeathsOthersTotal","EvacsTotal","DeathsConsecutive","EvacsConsecutive","KillsPlayersTotal","KillsPlayersMax","KillsCreaturesTotal","KillsCreaturesMax","DamagePlayersTotal","DamagePlayersMax","DamageCreaturesTotal","DamageCreaturesMax","ContractsCompletedKorolevTotal","ContractsCompletedOsirisTotal","ContractsCompletedICATotal","ContractsCompletedTotal","MatchDurationMax","MatchDurationAvg","KMarksMatchMax","KMarksMatchTotal","KMarksTotal"]
public class GetPlayerStatisticsRequestsClientRequest
{
    [JsonPropertyName("statistics")]
    public string[] Statistics { get; set; }
}

public class GetPlayerStatisticsRequestsClientResponse
{
    // TODO: Unknown structure
}

[CloudScriptFunction("GetPlayerStatisticsRequestsClient")]
public class GetPlayerStatisticsRequestsClientFunction : ICloudScriptFunction<GetPlayerStatisticsRequestsClientRequest, GetPlayerStatisticsRequestsClientResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetPlayerStatisticsRequestsClientFunction(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GetPlayerStatisticsRequestsClientResponse> ExecuteAsync(GetPlayerStatisticsRequestsClientRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }

        return new GetPlayerStatisticsRequestsClientResponse
        {
        };
    }
}
