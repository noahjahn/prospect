using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;
using Prospect.Server.Api.Services.Auth.Extensions;
using Microsoft.AspNetCore.SignalR;
using Prospect.Server.Api.Hubs;

public class SquadMemberReadyForMatchRequest
{
    [JsonPropertyName("squadId")]
    public string SquadID { get; set; }
    [JsonPropertyName("matchmakingSettings")]
	public FYUserMatchmakingSettings matchmakingSettings { get; set; }
}

public class SquadMemberReadyForMatchResponse
{
    [JsonPropertyName("result")]
	public int Result { get; set; } // EYSquadActionResult
    [JsonPropertyName("squad")]
	public FYPlayFabSquad Squad { get; set; }
    [JsonPropertyName("isSquadReadyForMatch")]
	public bool IsSquadReadyForMatch { get; set; }
}

[CloudScriptFunction("SquadMemberReadyForMatch")]
public class SquadMemberReadyForMatchFunction : ICloudScriptFunction<SquadMemberReadyForMatchRequest, SquadMemberReadyForMatchResponse>
{
    private readonly ILogger<SquadMemberReadyForMatchFunction> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHubContext<CycleHub> _hubContext;

    public SquadMemberReadyForMatchFunction(ILogger<SquadMemberReadyForMatchFunction> logger, IHttpContextAccessor httpContextAccessor, IHubContext<CycleHub> hubContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<SquadMemberReadyForMatchResponse> ExecuteAsync(SquadMemberReadyForMatchRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        return new SquadMemberReadyForMatchResponse
        {
            Result = 0, // EYSquadActionResult::OK
            Squad = new FYPlayFabSquad {
                SquadID = "100", // TODO
                // Members = [
                //     new FYPlayFabSquadMember {
                //         Profile = new FYPlayFabPlayerProfile {
                //             PlayerId = userId,
                //         },
                //         onlineState = 0, // EYUserState::IN_STATION
                //         matchmakingSettings = new FYUserMatchmakingSettings {
                //             isReadyForMatch = true,
                //             isSecretLeader = true,
                //             selectedMapName = "Map01",
                //         }
                //     }
                // ],
            },
            IsSquadReadyForMatch = true,
        };
    }
}
