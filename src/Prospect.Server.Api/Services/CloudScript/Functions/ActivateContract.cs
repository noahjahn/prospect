using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models.Data;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYActivateContractRequest {
    [JsonPropertyName("contractId")]
    public string ContractID { get; set; }
}

public class FYActivateContractResult {
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
    [JsonPropertyName("changedCurrencties")]
    public object[] ChangedCurrencties { get; set; }
    [JsonPropertyName("activatedContract")]
    public FYActiveContractPlayerData ActivatedContract { get; set; }
    [JsonPropertyName("status")]
    public int Status { get; set; }
}

public class FYGetActiveContractsResult {
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
    [JsonPropertyName("contracts")]
    public List<FYActiveContractPlayerData> Contracts { get; set; }
}

[CloudScriptFunction("ActivateContract")]
public class ActivateContract : ICloudScriptFunction<FYActivateContractRequest, FYActivateContractResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public ActivateContract(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYActivateContractResult> ExecuteAsync(FYActivateContractRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"ContractsActive"});
        var titleData = _titleDataService.Find(new List<string>{"Contracts"});

        var contracts = JsonSerializer.Deserialize<Dictionary<string, TitleDataContractInfo>>(titleData["Contracts"]);
        if (!contracts.TryGetValue(request.ContractID, out var contract)) {
            return new FYActivateContractResult
            {
                UserID = userId,
                Error = "Contract not found",
                Status = 3 // EYActivateContractRequestStatus::FAILED_GETTING_STATIC_DATA
            };
        }
        // TODO: Contract unlock criteria

        var progress = new int[contract.Objectives.Length];
        // TODO: Automatically set max progress of kill and visit objectives. To remove once kill and visit objectives are reported by the client.
        for (var i = 0; i < contract.Objectives.Length; i++) {
            var objective = contract.Objectives[i];
            if (objective.Type != EYContractObjectiveType.OwnNumOfItem) {
                progress[i] = objective.MaxProgress;
            }
        }

        var contractsActive = JsonSerializer.Deserialize<FYGetActiveContractsResult>(userData["ContractsActive"].Value);
        var contractData = new FYActiveContractPlayerData {
            ContractID = request.ContractID,
            Progress = progress,
        };
        contractsActive.Contracts.Add(contractData);
        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{ ["ContractsActive"] = JsonSerializer.Serialize(contractsActive) }
        );

        return new FYActivateContractResult
        {
            UserID = userId,
            Error = "",
            ChangedCurrencties = [], // NOTE: There are no paid contracts in Frontier.
            ActivatedContract = contractData,
            Status = 14 // EYActivateContractRequestStatus::OK
        };
    }
}