using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript.Models;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYGetCharacterVanityResponse {
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("returnVanity")]
    public FYCharacterVanity ReturnVanity { get; set; }
}

public class FYVanityMaterialItem {
    [JsonPropertyName("id")]
    public string ID { get; set; }
    [JsonPropertyName("materialIndex")]
    public int MaterialIndex { get; set; }
}

public class FYCharacterVanity {
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("head_item")]
    public FYVanityMaterialItem HeadItem { get; set; }
    [JsonPropertyName("boots_item")]
    public FYVanityMaterialItem BootsItem { get; set; }
    [JsonPropertyName("chest_item")]
    public FYVanityMaterialItem ChestItem { get; set; }
    [JsonPropertyName("glove_item")]
    public FYVanityMaterialItem GloveItem { get; set; }
    [JsonPropertyName("base_suit_item")]
    public FYVanityMaterialItem BaseSuitItem { get; set; }
    [JsonPropertyName("melee_weapon_item")]
    public FYVanityMaterialItem MeleeWeaponItem { get; set; }
    [JsonPropertyName("body_type")]
    public int BodyType { get; set; }
    [JsonPropertyName("archetype_id")]
    public string ArchetypeID { get; set; }
    [JsonPropertyName("slot_index")]
    public int SlotIndex { get; set; }
}

[CloudScriptFunction("GetCharacterVanity")]
public class GetCharacterVanity : ICloudScriptFunction<FYGetCharacterVanityRequest, FYGetCharacterVanityResponse?>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;

    public GetCharacterVanity(IHttpContextAccessor httpContextAccessor, UserDataService userDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
    }

    public async Task<FYGetCharacterVanityResponse?> ExecuteAsync(FYGetCharacterVanityRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return null;
        }
        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"CharacterVanity"});
        var returnVanity = JsonSerializer.Deserialize<FYCharacterVanity>(userData["CharacterVanity"].Value);

        return new FYGetCharacterVanityResponse{
            Success = true,
            ReturnVanity = returnVanity,
        };
    }
}