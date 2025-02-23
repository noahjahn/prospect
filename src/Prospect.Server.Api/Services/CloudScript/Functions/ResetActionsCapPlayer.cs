using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models.Data;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class ResetActionsCapPlayerRequest
{
    // Empty request
}

public class ResetActionsCapPlayerResponse
{
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
}

public class FortunaPassDailyCompletedActions {
    [JsonPropertyName("actions")]
    public object Actions { get; set; }
    [JsonPropertyName("lastDailyCapResetTimeUtc")]
    public FYTimestamp LastDailyCapResetTimeUtc { get; set; }
}

[CloudScriptFunction("ResetActionsCapPlayer")]
public class ResetActionsCapPlayer : ICloudScriptFunction<ResetActionsCapPlayerRequest, ResetActionsCapPlayerResponse>
{
    private readonly UserDataService _userDataService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ResetActionsCapPlayer(IHttpContextAccessor httpContextAccessor, UserDataService userDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
    }

    public async Task<ResetActionsCapPlayerResponse> ExecuteAsync(ResetActionsCapPlayerRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"FortunaPass2_DailyCompletedActions"});

        var dailyCompletedActions = JsonSerializer.Deserialize<FortunaPassDailyCompletedActions>(userData["FortunaPass2_DailyCompletedActions"].Value);
        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now <= dailyCompletedActions.LastDailyCapResetTimeUtc.Seconds + 86400) {
            return new ResetActionsCapPlayerResponse {
                UserID = userId,
                Error = "",
            };
        }

        dailyCompletedActions.LastDailyCapResetTimeUtc.Seconds = now;
        await _userDataService.UpdateAsync(userId, userId, new Dictionary<string, string>{
            ["FortunaPass2_DailyCompletedActions"] = JsonSerializer.Serialize(dailyCompletedActions), // Season 2
            ["FortunaPass3_DailyCompletedActions"] = JsonSerializer.Serialize(dailyCompletedActions), // Season 3
        });

        return new ResetActionsCapPlayerResponse{
            UserID = userId,
            Error = "",
        };
    }
}