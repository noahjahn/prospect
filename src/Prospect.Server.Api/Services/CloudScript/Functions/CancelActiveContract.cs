using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYCancelActiveContractRequest {
    [JsonPropertyName("contractId")]
    public string ContractID { get; set; }
}

public class FYCancelActiveContractResult {
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
    [JsonPropertyName("contractId")]
    public string ContractID { get; set; }
}

[CloudScriptFunction("CancelActiveContract")]
public class CancelActiveContract : ICloudScriptFunction<FYCancelActiveContractRequest, FYCancelActiveContractResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;

    public CancelActiveContract(IHttpContextAccessor httpContextAccessor, UserDataService userDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
    }

    public async Task<FYCancelActiveContractResult> ExecuteAsync(FYCancelActiveContractRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"ContractsActive"});
        var contractsActive = JsonSerializer.Deserialize<FYGetActiveContractsResult>(userData["ContractsActive"].Value);
        var contractActiveIdx = contractsActive.Contracts.FindIndex(item => item.ContractID == request.ContractID);
        if (contractActiveIdx == -1) {
            return new FYCancelActiveContractResult
            {
                UserID = userId,
                Error = "Failed to find active contract",
            };
        }
        contractsActive.Contracts.RemoveAt(contractActiveIdx);
        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{ ["ContractsActive"] = JsonSerializer.Serialize(contractsActive) }
        );

        return new FYCancelActiveContractResult
        {
            UserID = userId,
            Error = "",
            ContractID = request.ContractID,
        };
    }
}