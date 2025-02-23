using System.Text.Json;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models;
using Prospect.Server.Api.Services.CloudScript.Models.Data;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

[CloudScriptFunction("RequestActiveObjectivesAndBoardsData")]
public class RequestActiveObjectivesAndBoardsData : ICloudScriptFunction<FYQueryFactionProgressionRequest, FYGetPlayerContractsResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;

    public RequestActiveObjectivesAndBoardsData(IHttpContextAccessor httpContextAccessor, UserDataService userDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
    }

    public async Task<FYGetPlayerContractsResult> ExecuteAsync(FYQueryFactionProgressionRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return new FYGetPlayerContractsResult{};
        }
        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"ContractsActive"});
        var contractsActive = JsonSerializer.Deserialize<FYGetActiveContractsResult>(userData["ContractsActive"].Value);

        // TODO: Get player's contract boards and refresh

        return new FYGetPlayerContractsResult
        {
            UserId = userId,
            Error = null,
            ActiveContracts = contractsActive.Contracts,
            FactionsContracts = new FYFactionsContractsData
            {
                Boards = new List<FYFactionContractsData>
                {
                    new FYFactionContractsData
                    {
                        FactionId = "ICA",
                        Contracts = new List<FYFactionContractData>
                        {
                            new FYFactionContractData
                            {
                                ContractId = "NEW-Easy-ICA-Gather-1",
                            },
                            new FYFactionContractData
                            {
                                ContractId = "NEW-Medium-ICA-Uplink-1",
                            },
                            new FYFactionContractData
                            {
                                ContractId = "NEW-Hard-ICA-Uplink-1",
                            }
                        }
                    },
                    new FYFactionContractsData
                    {
                        FactionId = "Korolev",
                        Contracts = new List<FYFactionContractData>
                        {
                            new FYFactionContractData
                            {
                                ContractId = "NEW-Easy-KOR-Mine-4",
                            },
                            new FYFactionContractData
                            {
                                ContractId = "NEW-Medium-KOR-Mine-1",
                            },
                            new FYFactionContractData
                            {
                                ContractId = "NEW-Hard-KOR-PvP-6",
                            }
                        }
                    },
                    new FYFactionContractsData
                    {
                        FactionId = "Osiris",
                        Contracts = new List<FYFactionContractData>
                        {
                            new FYFactionContractData
                            {
                                ContractId = "NEW-Easy-Osiris-Brightcaps-1",
                            },
                            new FYFactionContractData
                            {
                                ContractId = "NEW-Medium-Osiris-Gather-1",
                            },
                            new FYFactionContractData
                            {
                                ContractId = "NEW-Hard-Osiris-Gather-7",
                            }
                        }
                    }
                },
                LastBoardRefreshTimeUtc = new FYTimestamp
                {
                    Seconds = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            },
            RefreshHours24UtcFromBackend = 12
        };
    }
}