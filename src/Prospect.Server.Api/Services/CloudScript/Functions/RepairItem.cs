using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.UserData;
using Prospect.Server.Api.Utils;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYRepairItemRequest {
    [JsonPropertyName("instanceId")]
	public string InstanceID { get; set; }
}

public class FYRepairItemResult {
    [JsonPropertyName("userId")]
	public string UserID { get; set; }
    [JsonPropertyName("error")]
	public string Error { get; set; }
    [JsonPropertyName("changedCurrencies")]
	public FYCurrencyItem[] ChangedCurrencies { get; set; }
    [JsonPropertyName("changedItems")]
	public FYCustomItemInfo[] ChangedItems { get; set; }
}

[CloudScriptFunction("RepairItem")]
public class RepairItem : ICloudScriptFunction<FYRepairItemRequest, FYRepairItemResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public RepairItem(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<FYRepairItemResult> ExecuteAsync(FYRepairItemRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var titleData = _titleDataService.Find(new List<string>{"Blueprints"});

        var userData = await _userDataService.FindAsync(
            userId, userId,
            new List<string>{"Balance", "Inventory"}
        );

        var inventory = JsonSerializer.Deserialize<List<FYCustomItemInfo>>(userData["Inventory"].Value);

        var item = inventory.Find(item => item.ItemId == request.InstanceID);
        if (item == null) {
            return new FYRepairItemResult
            {
                UserID = userId,
                Error = "Item not found",
            };
        }

        var blueprints = JsonSerializer.Deserialize<Dictionary<string, TitleDataBlueprintInfo>>(titleData["Blueprints"]);
        var blueprintData = blueprints[item.BaseItemId];

        var balance = JsonSerializer.Deserialize<Dictionary<string, int>>(userData["Balance"].Value);

        var repairCost = MapValue.Map(item.Durability, blueprintData.DurabilityMax, 0, blueprintData.RepairCostBase, blueprintData.RepairCostMaxDurability);
        if (item.Durability == 0) {
            repairCost += blueprintData.RepairCostModifierBroken;
        }
        if (balance["SC"] < repairCost) {
            return new FYRepairItemResult
            {
                UserID = userId,
                Error = "Insufficient balance",
            };
        }
        balance["SC"] -= repairCost;
        item.Durability = blueprintData.DurabilityMax;

        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{
                ["Inventory"] = JsonSerializer.Serialize(inventory),
                ["Balance"] = JsonSerializer.Serialize(balance),
            }
        );

        return new FYRepairItemResult
        {
            UserID = userId,
            Error = "",
            ChangedItems = [item],
            ChangedCurrencies = [
                new FYCurrencyItem { CurrencyName = "SoftCurrency", Amount = balance["SC"] },
            ],
        };
    }
}