using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYRequestMissionCompletedRequest {
    [JsonPropertyName("currentMissionId")]
    public string CurrentMissionID { get; set; }
}

public class FYRequestMissionCompletedResult {
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("currentMissionId")]
    public string CurrentMissionID { get; set; }
    [JsonPropertyName("rewards")]
    public List<FYCustomItemInfo> Rewards { get; set; }
    [JsonPropertyName("updatedCurrencies")]
    public FYCurrencyItem[] UpdatedCurrencies { get; set; }
    [JsonPropertyName("progress")]
    public int Progress { get; set; }
}

[CloudScriptFunction("RequestMissionCompleted")]
public class RequestMissionCompleted : ICloudScriptFunction<FYRequestMissionCompletedRequest, FYRequestMissionCompletedResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public RequestMissionCompleted(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYRequestMissionCompletedResult> ExecuteAsync(FYRequestMissionCompletedRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"OnboardingProgression", "Inventory", "Balance"});
        var onboardingMission = JsonSerializer.Deserialize<OnboardingMission>(userData["OnboardingProgression"].Value);
        var inventory = JsonSerializer.Deserialize<List<FYCustomItemInfo>>(userData["Inventory"].Value);

        var titleData = _titleDataService.Find(new List<string>{"OnboardingMissions", "OnboardingRewardMissions", "Blueprints"});
        var rewards = JsonSerializer.Deserialize<Dictionary<string, TitleDataMissionRewardInfo>>(titleData["OnboardingRewardMissions"]);
        var blueprints = JsonSerializer.Deserialize<Dictionary<string, TitleDataBlueprintInfo>>(titleData["Blueprints"]);
        var missions = JsonSerializer.Deserialize<Dictionary<string, TitleDataMissionInfo>>(titleData["OnboardingMissions"]);
        var balance = JsonSerializer.Deserialize<Dictionary<string, int>>(userData["Balance"].Value);

        var currentMission = missions[onboardingMission.CurrentMissionID];
        var missionRewardId = currentMission.OnboardingRewards.RowName;
        var nextMissionId = currentMission.NextMissionRowHandle.RowName;
        onboardingMission.CurrentMissionID = nextMissionId;
        onboardingMission.Progress = 0;

        List<FYCustomItemInfo> rewardedItems = new List<FYCustomItemInfo>();
        List<FYCurrencyItem> changedCurrencies = [];
        if (rewards.ContainsKey(missionRewardId)) {
            var missionRewards = rewards[missionRewardId].RewardEntries;
            foreach (var reward in missionRewards) {
                if (reward.RewardRowHandle.RowName == "SoftCurrency") {
                    balance["SC"] += reward.RewardAmount;
                    changedCurrencies.Add(new FYCurrencyItem { CurrencyName = "SoftCurrency", Amount = balance["SC"] });
                } else if (reward.RewardRowHandle.RowName == "Aurum") {
                    balance["AU"] += reward.RewardAmount;
                    changedCurrencies.Add(new FYCurrencyItem { CurrencyName = "Aurum", Amount = balance["AU"] });
                } else if (reward.RewardRowHandle.RowName == "InsuranceToken") {
                    balance["IN"] += reward.RewardAmount;
                    changedCurrencies.Add(new FYCurrencyItem { CurrencyName = "InsuranceToken", Amount = balance["IN"] });
                } else {
                    var blueprintData = blueprints[reward.RewardRowHandle.RowName];
                    var remainingAmount = reward.RewardAmount * blueprintData.AmountPerPurchase;
                    while (remainingAmount > 0) {
                        var itemStackAmount = Math.Min(blueprintData.MaxAmountPerStack, remainingAmount);
                        // TODO: Mission rewards are probably already given in stacks
                        var itemInfo = new FYCustomItemInfo{
                            ItemId = Guid.NewGuid().ToString(),
                            Amount = itemStackAmount,
                            BaseItemId = reward.RewardRowHandle.RowName,
                            Durability = blueprintData.DurabilityMax,
                            Insurance = "",
                            InsuranceOwnerPlayfabId = "",
                            ModData = new FYModItems { M = [] },
                            Origin = new FYItemOriginBackend { G = "", P = "", T = "" },
                            InsuredAttachmentId = "",
                            PrimaryVanityId = 0,
                            SecondaryVanityId = 0,
                            RolledPerks = [],
                        };
                        inventory.Add(itemInfo);
                        rewardedItems.Add(itemInfo);
                        remainingAmount -= itemStackAmount;
                    }
                }
            }
        }

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["OnboardingProgression"] = JsonSerializer.Serialize(onboardingMission),
                ["Inventory"] = JsonSerializer.Serialize(inventory),
                ["Balance"] = JsonSerializer.Serialize(balance),
            }
        );

        return new FYRequestMissionCompletedResult
        {
            UserID = userId,
            CurrentMissionID = nextMissionId,
            Rewards = rewardedItems,
            UpdatedCurrencies = changedCurrencies.ToArray(),
            Progress = 0
        };
    }
}