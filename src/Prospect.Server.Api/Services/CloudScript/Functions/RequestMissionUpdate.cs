using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYRequestMissionUpdateRequest {
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("currentMissionId")]
    public string CurrentMissionID { get; set; }
    [JsonPropertyName("currentStepName")]
    public string CurrentStepName { get; set; }
    [JsonPropertyName("progress")]
    public int Progress { get; set; }
    [JsonPropertyName("currentRoomId")]
    public string CurrentRoomID { get; set; }
}

public class FYRequestMissionUpdateResult {
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
    [JsonPropertyName("currentMissionId")]
    public string CurrentMissionID { get; set; }
    [JsonPropertyName("rewards")]
    public object[] Rewards { get; set; }
    [JsonPropertyName("progress")]
    public int Progress { get; set; }
}

public class OnboardingMission {
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("currentMissionID")]
    public string CurrentMissionID { get; set; }
    [JsonPropertyName("progress")]
    public int Progress { get; set; }
    [JsonPropertyName("showPopup")]
    public bool ShowPopup { get; set; }
}

[CloudScriptFunction("RequestMissionUpdate")]
public class RequestMissionUpdate : ICloudScriptFunction<FYRequestMissionUpdateRequest, FYRequestMissionUpdateResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;

    public RequestMissionUpdate(IHttpContextAccessor httpContextAccessor, UserDataService userDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
    }

    public async Task<FYRequestMissionUpdateResult> ExecuteAsync(FYRequestMissionUpdateRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"OnboardingProgression"});
        var onboardingMission = JsonSerializer.Deserialize<OnboardingMission>(userData["OnboardingProgression"].Value);
        onboardingMission.CurrentMissionID = request.CurrentMissionID;
        onboardingMission.Progress = request.Progress;
        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{ ["OnboardingProgression"] = JsonSerializer.Serialize(onboardingMission) }
        );

        return new FYRequestMissionUpdateResult
        {
            UserID = userId,
            Error = "",
            CurrentMissionID = request.CurrentMissionID,
            Rewards = [],
            Progress = request.Progress
        };
    }
}