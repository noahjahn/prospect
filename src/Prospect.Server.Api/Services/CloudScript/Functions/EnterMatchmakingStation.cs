using Prospect.Server.Api.Services.CloudScript.Models;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

[CloudScriptFunction("EnterMatchmakingStation")]
public class EnterMatchmakingStationFunction : ICloudScriptFunction<FYEnterMatchAzureFunction, FYEnterMatchmakingResult>
{
    public Task<FYEnterMatchmakingResult> ExecuteAsync(FYEnterMatchAzureFunction request)
    {
        return Task.FromResult(new FYEnterMatchmakingResult
        {
            Success = true,
            ErrorMessage = "",
            SingleplayerStation = true, // NOTE: This will always travel to single player station
            IsMatchTravel = false,
        });
    }
}