using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Prospect.Server.Api.Hubs;
using Prospect.Server.Api.Services.CloudScript.Models;

// Closed Beta Function

namespace Prospect.Server.Api.Services.CloudScript.Functions;

[CloudScriptFunction("EnterMatchmaking")]
public class EnterMatchmakingFunction : ICloudScriptFunction<FYEnterMatchAzureFunction, FYEnterMatchAzureFunctionResult>
{
    private readonly IHubContext<CycleHub> _hubContext;

    public EnterMatchmakingFunction(IHubContext<CycleHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<FYEnterMatchAzureFunctionResult> ExecuteAsync(FYEnterMatchAzureFunction request)
    {
        //await _hubContext.Clients.All.SendAsync("OnSquadMatchmakingSuccess", new OnSquadMatchmakingSuccessMessage {
        //    Success = true,
        //    SessionID = request.MapName, // TODO: Need to implement TryGetCompleteSquadInfo and pass squad info
        //    SquadID = request.SquadId,
        //});

        return new FYEnterMatchAzureFunctionResult
        {
            Success = true,
            ErrorMessage = "",
            SingleplayerStation = true,
            Address = request.MapName,
            MaintenanceMode = false,
            Port = 7777,
        };
    }
}