using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Prospect.Server.Api.Hubs;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models;
using Prospect.Server.Api.Services.UserData;

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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHubContext<CycleHub> _hubContext;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public EnterMatchmakingMatchFunction(IHubContext<CycleHub> hubContext, IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _hubContext = hubContext;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYEnterMatchmakingResult> ExecuteAsync(FYEnterMatchAzureFunction request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }

        var userId = context.User.FindAuthUserId();
        var titleData = _titleDataService.Find(new List<string>{"Contracts"});
        var contracts = JsonSerializer.Deserialize<Dictionary<string, TitleDataContractInfo>>(titleData["Contracts"]);

        var userData = await _userDataService.FindAsync(
            userId, userId,
            new List<string>{"ContractsActive", "Inventory"}
        );
        var inventory = JsonSerializer.Deserialize<List<FYCustomItemInfo>>(userData["Inventory"].Value);
        var contractsActive = JsonSerializer.Deserialize<FYGetActiveContractsResult>(userData["ContractsActive"].Value);
        // Compute delivery quest progress before deploy.
        // This ensures that the quest progress will be shown on the planet.
        // The station doesn't seem to care about delivery quests progress and calculates progress
        // based on actual items in stash. And so does the "ClaimActiveContract" function.
        foreach (var contractActive in contractsActive.Contracts) {
            if (!contracts.TryGetValue(contractActive.ContractID, out var contract)) {
                continue;
            }
            for (var i = 0; i < contract.Objectives.Length; i++) {
                var objective = contract.Objectives[i];
                if (objective.Type != EYContractObjectiveType.OwnNumOfItem) {
                    continue;
                }
                int remaining = objective.MaxProgress;
                foreach (var item in inventory) {
                    if (item.BaseItemId != objective.ItemToOwn) {
                        continue;
                    }
                    remaining -= item.Amount;
                    if (remaining <= 0) {
                        remaining = 0;
                        break;
                    }
                }
                contractActive.Progress[i] = objective.MaxProgress - remaining;
            }
        }

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["ContractsActive"] = JsonSerializer.Serialize(contractsActive),
            }
        );

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