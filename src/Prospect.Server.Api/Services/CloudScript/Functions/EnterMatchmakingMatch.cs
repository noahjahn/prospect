using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Prospect.Server.Api.Hubs;
using Prospect.Server.Api.Services.CloudScript.Models;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class OnSquadMatchmakingSuccessMessage {
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    [JsonPropertyName("sessionId")]
    public string SessionID { get; set; }
    [JsonPropertyName("squadId")]
    public string SquadID { get; set; }
}

[CloudScriptFunction("EnterMatchmakingMatch")]
public class EnterMatchmakingMatchFunction : ICloudScriptFunction<FYEnterMatchAzureFunction, FYEnterMatchmakingResult>
{
    private readonly IHubContext<CycleHub> _hubContext;

    public EnterMatchmakingMatchFunction(IHubContext<CycleHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<FYEnterMatchmakingResult> ExecuteAsync(FYEnterMatchAzureFunction request)
    {
        await _hubContext.Clients.All.SendAsync("OnSquadMatchmakingSuccess", new OnSquadMatchmakingSuccessMessage {
            Success = true,
            SessionID = request.MapName, // TODO: Need to implement TryGetCompleteSquadInfo and pass squad info
            SquadID = request.SquadId,
        });

        return new FYEnterMatchmakingResult
        {
            Success = true,
            ErrorMessage = "",
            SingleplayerStation = false,
            NumAttempts = 1,
            Blocker = 0,
            IsMatchTravel = true,
            SessionId = "", // TODO: Not sure how this affects match travel
        };
    }
}