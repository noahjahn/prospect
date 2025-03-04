using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.UserData;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

public class FYSetActiveCharacterArchetypeRequest
{
    [JsonPropertyName("archetypeId")]
    public string ArcheTypeId { get; set; } // E03_G02
}

public class FYSetActiveCharacterArchetypeResponse
{
    [JsonPropertyName("success")]
	public bool Success { get; set; }
    [JsonPropertyName("returnVanity")]
	public FYCharacterVanity ReturnVanity { get; set; }
}

[CloudScriptFunction("SetCharacterVanityArchetype")]
public class SetCharacterVanityArchetype : ICloudScriptFunction<FYSetActiveCharacterArchetypeRequest, FYSetActiveCharacterArchetypeResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;

    public SetCharacterVanityArchetype(IHttpContextAccessor httpContextAccessor, UserDataService userDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
    }

    public async Task<FYSetActiveCharacterArchetypeResponse> ExecuteAsync(FYSetActiveCharacterArchetypeRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return new FYSetActiveCharacterArchetypeResponse{};
        }

        var userId = context.User.FindAuthUserId();
        var userData = await _userDataService.FindAsync(userId, userId, new List<string>{"CharacterVanity"});
        var returnVanity = JsonSerializer.Deserialize<FYCharacterVanity>(userData["CharacterVanity"].Value);

        // TODO: This works only for Season 2. Season 3 has different IDs
        var genderSuffix = request.ArcheTypeId.EndsWith("G02") ? "M" : "F";
        returnVanity.ArchetypeID = request.ArcheTypeId;
        returnVanity.HeadItem.ID = request.ArcheTypeId + "_Head01";
#if SEASON_2_RELEASE || SEASON_2_DEBUG
        returnVanity.BootsItem.ID = $"StarterOutfit01_Boots_{genderSuffix}";
        returnVanity.ChestItem.ID = $"StarterOutfit01_Chest_{genderSuffix}";
        returnVanity.GloveItem.ID = $"StarterOutfit01_Gloves_{genderSuffix}";
#elif SEASON_3_RELEASE || SEASON_3_DEBUG
        returnVanity.BootsItem.ID = "StarterOutfit01_Boots";
        returnVanity.ChestItem.ID = "StarterOutfit01_Chest";
        returnVanity.GloveItem.ID = "StarterOutfit01_Gloves";
#else
#error Unsupported build type
#endif
        returnVanity.BaseSuitItem.ID = $"StarterOutfit01{genderSuffix}_BaseSuit";
        returnVanity.BodyType = request.ArcheTypeId.EndsWith("G02") ? 1 : 2;
        await _userDataService.UpdateAsync(
            userId, userId,
            new Dictionary<string, string>{ ["CharacterVanity"] = JsonSerializer.Serialize(returnVanity) }
        );

        return new FYSetActiveCharacterArchetypeResponse
        {
            Success = true,
            ReturnVanity = returnVanity,
        };
    }
}