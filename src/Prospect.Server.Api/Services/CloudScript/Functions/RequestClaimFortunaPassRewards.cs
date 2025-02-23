using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;
using Prospect.Server.Api.Services.UserData;

public class FYFortunaPassClaimedRewards {
    [JsonPropertyName("rewardsIds")]
	public List<string> RewardsIDs { get; set; }
};

public class RequestClaimFortunaPassRewardsRequest
{
    [JsonPropertyName("rewardsIds")]
    public string[] RewardsIDs { get; set; }
}

public class RequestClaimFortunaPassRewardsResponse
{
    [JsonPropertyName("userIds")]
    public string UserId { get; set; }
    [JsonPropertyName("error")]
	public string Error { get; set; }
    [JsonPropertyName("errorType")]
	public int ErrorType { get; set; }
    [JsonPropertyName("grantedItems")]
	public FYCustomItemInfo[] GrantedItems { get; set; }
    [JsonPropertyName("changedCurrencies")]
	public FYCurrencyItem[] ChangedCurrencies { get; set; }
}

[CloudScriptFunction("RequestClaimFortunaPassRewards")]
public class RequestClaimFortunaPassRewardsFunction : ICloudScriptFunction<RequestClaimFortunaPassRewardsRequest, RequestClaimFortunaPassRewardsResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;

    public RequestClaimFortunaPassRewardsFunction(IHttpContextAccessor httpContextAccessor, UserDataService userDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
    }

    public async Task<RequestClaimFortunaPassRewardsResponse> ExecuteAsync(RequestClaimFortunaPassRewardsRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"FortunaPass2_ClaimedRewards"});
        var claimedRewards = JsonSerializer.Deserialize<FYFortunaPassClaimedRewards>(userData["FortunaPass2_ClaimedRewards"].Value);
        foreach (var rewardId in request.RewardsIDs) {
            claimedRewards.RewardsIDs.Add(rewardId);
        }

        var output = JsonSerializer.Serialize(claimedRewards);
        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["FortunaPass2_ClaimedRewards"] = output,
                ["FortunaPass3_ClaimedRewards"] = output, // TODO: Separate logic for S2 and S3
            }
        );

        return new RequestClaimFortunaPassRewardsResponse
        {
            UserId = userId,
            ErrorType = 3, // EYFortunaPassToastReponseType::Ok
            Error = "",
            GrantedItems = [],
            ChangedCurrencies = [],
        };
    }
}
