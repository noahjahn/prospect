using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models;
using Prospect.Server.Api.Services.CloudScript.Models.Data;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

[CloudScriptFunction("RequestPlayerContracts")]
public class RequestPlayerContracts : ICloudScriptFunction<FYGetPlayerContractsRequest, FYGetPlayerContractsResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestPlayerContracts(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<FYGetPlayerContractsResult> ExecuteAsync(FYGetPlayerContractsRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }

        return Task.FromResult(new FYGetPlayerContractsResult
        {
            UserId = context.User.FindAuthUserId(),
            Error = null,
            ActiveContracts = new List<FYActiveContractPlayerData>(),
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
        });
    }
}