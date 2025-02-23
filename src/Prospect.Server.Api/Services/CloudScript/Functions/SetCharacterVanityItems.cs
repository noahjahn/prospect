using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;
using Prospect.Server.Api.Services.CloudScript.Functions;
using Prospect.Server.Api.Services.UserData;

public class VanityDataiD {
    [JsonPropertyName("iD")]
    public string ID { get; set; }
    [JsonPropertyName("materialIndex")]
    public int MaterialIndex { get; set; }
}

public class SetCharacterVanityItems {
    [JsonPropertyName("userId")]
    public string UserID { get; set; }

    [JsonPropertyName("head_item")]
    public VanityDataiD HeadItem { get; set; }

    [JsonPropertyName("boots_item")]
    public VanityDataiD BootsItem { get; set; }
    [JsonPropertyName("chest_item")]
    public VanityDataiD ChestItem { get; set; }
    [JsonPropertyName("glove_item")]
    public VanityDataiD GloveItem { get; set; }
    [JsonPropertyName("base_suit_item")]
    public VanityDataiD BaseSuitItem { get; set; }
    [JsonPropertyName("melee_weapon_item")]
    public VanityDataiD MeleeWeaponItem { get; set; }
    [JsonPropertyName("body_type")]
    public int BodyType { get; set; }
    [JsonPropertyName("archetype_id")]
    public string ArchetypeID { get; set; }
    [JsonPropertyName("slot_index")]
    public int SlotIndex { get; set; }
}

public class SetCharacterVanityItemsRequest {
    [JsonPropertyName("desiredVanity")]
    public SetCharacterVanityItems DesiredVanity { get; set; }
}

public class SetCharacterVanityItemsResponse
{
    [JsonPropertyName("returnVanity")]
    public FYCharacterVanity ReturnVanity { get; set; }
}

[CloudScriptFunction("SetCharacterVanityItems")]
public class SetCharacterVanityItemsFunction : ICloudScriptFunction<SetCharacterVanityItemsRequest, SetCharacterVanityItemsResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;

    public SetCharacterVanityItemsFunction(IHttpContextAccessor httpContextAccessor, UserDataService userDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
    }

    public async Task<SetCharacterVanityItemsResponse> ExecuteAsync(SetCharacterVanityItemsRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        // TODO: Check for available vanity items
        // TODO: Refactor
        var vanityData = request.DesiredVanity;
        var mappedVanityItems = new FYCharacterVanity{
            UserID = userId,
            BaseSuitItem = new FYVanityMaterialItem{
                ID = vanityData.BaseSuitItem.ID,
                MaterialIndex = vanityData.BaseSuitItem.MaterialIndex,
            },
            BootsItem = new FYVanityMaterialItem{
                ID = vanityData.BootsItem.ID,
                MaterialIndex = vanityData.BootsItem.MaterialIndex,
            },
            ChestItem = new FYVanityMaterialItem{
                ID = vanityData.ChestItem.ID,
                MaterialIndex = vanityData.ChestItem.MaterialIndex,
            },
            GloveItem = new FYVanityMaterialItem{
                ID = vanityData.GloveItem.ID,
                MaterialIndex = vanityData.GloveItem.MaterialIndex,
            },
            HeadItem = new FYVanityMaterialItem{
                ID = vanityData.HeadItem.ID,
                MaterialIndex = vanityData.HeadItem.MaterialIndex,
            },
            MeleeWeaponItem = new FYVanityMaterialItem{
                ID = vanityData.MeleeWeaponItem.ID,
                MaterialIndex = vanityData.MeleeWeaponItem.MaterialIndex,
            },
            ArchetypeID = vanityData.ArchetypeID,
            BodyType = vanityData.BodyType,
            SlotIndex = vanityData.SlotIndex
        };
        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{ ["CharacterVanity"] = JsonSerializer.Serialize(mappedVanityItems) }
        );

        return new SetCharacterVanityItemsResponse
        {
            ReturnVanity = mappedVanityItems,
        };
    }
}
