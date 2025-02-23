using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;
using Prospect.Server.Api.Services.CloudScript.Models;
using Prospect.Server.Api.Services.UserData;

public class LoadoutData {
    [JsonPropertyName("shield")]
    public string Shield {  get; set; }
    [JsonPropertyName("helmet")]
    public string Helmet { get; set; }
    [JsonPropertyName("weaponOne")]
    public string WeaponOne { get; set; }
    [JsonPropertyName("weaponTwo")]
    public string WeaponTwo { get; set; }
    [JsonPropertyName("bag")]
    public string Bag { get; set; }
    [JsonPropertyName("bagItemsAsJsonStr")]
    public string BagItemsAsJsonStr { get; set; }
    [JsonPropertyName("safeItemsAsJsonStr")]
    public string SafeItemsAsJsonStr { get; set; }
}

public class RequestUpdateStationInventoryRequest
{
    [JsonPropertyName("newSet")]
    public LoadoutData NewSet { get; set; }
    [JsonPropertyName("itemsToAdd")]
    public FYCustomItemInfo[] ItemsToAdd { get; set; }
    [JsonPropertyName("itemsToUpdateAmount")]
    public FYCustomItemInfo[] ItemsToUpdateAmount { get; set; }
    [JsonPropertyName("itemsToRemove")]
    public HashSet<string> ItemsToRemove { get; set; }
}

public class RequestUpdateStationInventoryResponse
{
    // [JsonPropertyName("userId")]
    // public string UserId {  get; set; }
    // [JsonPropertyName("error")]
    // public string Error {  get; set; }
    // [JsonPropertyName("newSet")]
    // public LoadoutData NewSet {  get; set; }
    // [JsonPropertyName("itemsToAdd")]
    // public FYCustomItemInfo[] ItemsToAdd { get; set; }
    // [JsonPropertyName("itemsToUpdateAmount")]
    // public string[] ItemsToUpdateAmount { get; set; }
    // [JsonPropertyName("itemsToRemove")]
    // public string[] ItemsToRemove { get; set; }
}

[CloudScriptFunction("RequestUpdateStationInventory")]
public class RequestUpdateStationInventoryFunction : ICloudScriptFunction<RequestUpdateStationInventoryRequest, RequestUpdateStationInventoryResponse>
{
    private readonly ILogger<RequestUpdateStationInventoryFunction> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;

    public RequestUpdateStationInventoryFunction(ILogger<RequestUpdateStationInventoryFunction> logger, IHttpContextAccessor httpContextAccessor, UserDataService userDataService)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
    }

    public async Task<RequestUpdateStationInventoryResponse> ExecuteAsync(RequestUpdateStationInventoryRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"Inventory"});
        var inventory = JsonSerializer.Deserialize<List<FYCustomItemInfo>>(userData["Inventory"].Value);

        // TODO: Optimize
        var newInventory = new List<FYCustomItemInfo>(inventory.Count);
        foreach (var item in inventory) {
            if (!request.ItemsToRemove.Contains(item.ItemId)) {
                newInventory.Add(item);
            }
        }

        // TODO: Check deleted items to see if other stacks/mods were updated correctly
        foreach (var item in request.ItemsToUpdateAmount) {
            var inventoryItem = newInventory.Find(i => i.ItemId == item.ItemId);
            if (inventoryItem == null) {
                continue;
            }
            inventoryItem.Amount = item.Amount;
            inventoryItem.ModData = item.ModData;
            inventoryItem.PrimaryVanityId = item.PrimaryVanityId;
            inventoryItem.SecondaryVanityId = item.SecondaryVanityId;
        }

        // TODO: Check updated items and validate new items
        foreach (var item in request.ItemsToAdd) {
            newInventory.Add(item);
        }

        await _userDataService.UpdateAsync(userId, userId, new Dictionary<string, string>{
            ["LOADOUT"] = JsonSerializer.Serialize(request.NewSet),
            ["Inventory"] = JsonSerializer.Serialize(newInventory),
        });

        return new RequestUpdateStationInventoryResponse
        {
            // UserId = userId,
            // Error = "",
            // NewSet = request.NewSet,
            // ItemsToAdd = [],
            // ItemsToRemove = request.ItemsToRemove,
            // ItemsToUpdateAmount = request.ItemsToUpdateAmount,
        };
    }
}
