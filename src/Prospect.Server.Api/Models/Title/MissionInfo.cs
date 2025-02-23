using System.Text.Json.Serialization;

public class TitleDataMissionInfo {
    [JsonPropertyName("MissionId")]
    public string MissionId { get; set; }
    [JsonPropertyName("NextMissionRowHandle")]
    public UDataTable NextMissionRowHandle { get; set; }
    [JsonPropertyName("OnboardingRewards")]
    public UDataTable OnboardingRewards { get; set; }
}

// {\"TalkToBadumReward\":{\"MissionRewardId\":\"TalkToBadumReward\",\"RewardEntries\":[{\"RewardType\":7,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/PRO_Weapons.PRO_Weapons\\u0027\",\"RowName\":\"WP_E_SMG_Bullet_01\"},\"RewardAmount\":2},{\"RewardType\":7,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/PRO_Weapons.PRO_Weapons\\u0027\",\"RowName\":\"WP_E_AR_Energy_01\"},\"RewardAmount\":1},{\"RewardType\":7,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/PRO_Weapons.PRO_Weapons\\u0027\",\"RowName\":\"WP_E_Pistol_Bullet_01\"},\"RewardAmount\":5},{\"RewardType\":7,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/PRO_Weapons.PRO_Weapons\\u0027\",\"RowName\":\"WP_E_SGun_Bullet_01\"},\"RewardAmount\":2},{\"RewardType\":6,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/AmmoTypes_DT.AmmoTypes_DT\\u0027\",\"RowName\":\"Light\"},\"RewardAmount\":6},{\"RewardType\":6,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/AmmoTypes_DT.AmmoTypes_DT\\u0027\",\"RowName\":\"Medium\"},\"RewardAmount\":4},{\"RewardType\":6,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/AmmoTypes_DT.AmmoTypes_DT\\u0027\",\"RowName\":\"Shotgun\"},\"RewardAmount\":2},{\"RewardType\":8,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/PRO_Abilities.PRO_Abilities\\u0027\",\"RowName\":\"ShockGrenade_02\"},\"RewardAmount\":2},{\"RewardType\":8,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/PRO_Abilities.PRO_Abilities\\u0027\",\"RowName\":\"Consumable_Health_01\"},\"RewardAmount\":5},{\"RewardType\":0,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/PRO_PlayerShield.PRO_PlayerShield\\u0027\",\"RowName\":\"Shield_01\"},\"RewardAmount\":5},{\"RewardType\":10,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/PRO_Bags.PRO_Bags\\u0027\",\"RowName\":\"Bag_01\"},\"RewardAmount\":5},{\"RewardType\":7,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/PRO_Weapons.PRO_Weapons\\u0027\",\"RowName\":\"TOOL_MineralScanner_01\"},\"RewardAmount\":2}]},\"FirstMatchReward\":{\"MissionRewardId\":\"FirstMatchReward\",\"RewardEntries\":[{\"RewardType\":2,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/Materials_DT.Materials_DT\\u0027\",\"RowName\":\"Insulation\"},\"RewardAmount\":4},{\"RewardType\":1,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/Currencies_DT.Currencies_DT\\u0027\",\"RowName\":\"SoftCurrency\"},\"RewardAmount\":1500},{\"RewardType\":2,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/Materials_DT.Materials_DT\\u0027\",\"RowName\":\"GenericCreatureDrop\"},\"RewardAmount\":3},{\"RewardType\":2,\"RewardRowHandle\":{\"DataTable\":\"DataTable\\u0027/Game/DataTables/Materials_DT.Materials_DT\\u0027\",\"RowName\":\"ICAScrip\"},\"RewardAmount\":1}]}}",

public class TitleDataMissionRewardInfo {
    [JsonPropertyName("MissionRewardId")]
    public string MissionRewardId { get; set; }
    [JsonPropertyName("RewardEntries")]
    public TitleDataMissionRewardEntry[] RewardEntries { get; set; }
}

public class TitleDataMissionRewardEntry {
    [JsonPropertyName("RewardType")]
    public int RewardType { get; set; }
    [JsonPropertyName("RewardRowHandle")]
    public UDataTable RewardRowHandle { get; set; }
    [JsonPropertyName("RewardAmount")]
    public int RewardAmount { get; set; }
}

/*
enum class EYRewardType : uint8_t
{
	EYRewardType__None             = 0,
	EYRewardType__Currency         = 1,
	EYRewardType__Material         = 2,
	EYRewardType__Reputation       = 3,
	EYRewardType__SeasonXP         = 4,
	EYRewardType__ProspectorLevel  = 5,
	EYRewardType__Ammo             = 6,
	EYRewardType__Weapon           = 7,
	EYRewardType__Consumable       = 8,
	EYRewardType__Armor            = 9,
	EYRewardType__Bag              = 10,
	EYRewardType__MAX              = 11
};
*/