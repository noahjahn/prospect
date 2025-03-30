using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models.Data;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

// Custom request for dedicated server implementation

public class FYUpdatePlayerActiveContractsRequest {
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("contracts")]
    public List<FYActiveContractPlayerData> Contracts { get; set; }
}

public class FYUpdatePlayerActiveContractsResult {
}

[CloudScriptFunction("UpdatePlayerActiveContracts")]
public class UpdatePlayerActiveContracts : ICloudScriptFunction<FYUpdatePlayerActiveContractsRequest, FYUpdatePlayerActiveContractsResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public UpdatePlayerActiveContracts(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYUpdatePlayerActiveContractsResult> ExecuteAsync(FYUpdatePlayerActiveContractsRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"ContractsActive"});
        var contractsActive = JsonSerializer.Deserialize<FYGetActiveContractsResult>(userData["ContractsActive"].Value);

        foreach (var contract in request.Contracts) {
            var foundContract = contractsActive.Contracts.Find(item => item.ContractID == contract.ContractID);
            if (foundContract == null) {
                continue;
            }
            foundContract.Progress = contract.Progress;
        }

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{ ["ContractsActive"] = JsonSerializer.Serialize(contractsActive) }
        );

        return new FYUpdatePlayerActiveContractsResult
        {};
    }
}