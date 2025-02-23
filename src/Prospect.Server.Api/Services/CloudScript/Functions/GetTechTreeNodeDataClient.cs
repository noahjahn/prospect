using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.Auth.Extensions;
using Prospect.Server.Api.Services.CloudScript;
using Prospect.Server.Api.Services.CloudScript.Functions;
using Prospect.Server.Api.Services.UserData;

public class GetTechTreeNodeDataClientRequest
{
    // Empty request
}

public class GetTechTreeNodeDataClientResponse
{
    [JsonPropertyName("userId")]
    public string UserID { get; set; }
    [JsonPropertyName("nodesData")]
    public FYTechTreeNodeStatus[] NodesData { get; set; }
    [JsonPropertyName("remainingTimeInSeconds")]
    public int RemainingTimeInSeconds { get; set; }
}

[CloudScriptFunction("GetTechTreeNodeDataClient")]
public class GetTechTreeNodeDataClientFunction : ICloudScriptFunction<GetTechTreeNodeDataClientRequest, GetTechTreeNodeDataClientResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserDataService _userDataService;
    private readonly TitleDataService _titleDataService;

    public GetTechTreeNodeDataClientFunction(IHttpContextAccessor httpContextAccessor, UserDataService userDataService, TitleDataService titleDataService)
    {
        _httpContextAccessor = httpContextAccessor;
        _userDataService = userDataService;
        _titleDataService = titleDataService;
    }

    public async Task<GetTechTreeNodeDataClientResponse> ExecuteAsync(GetTechTreeNodeDataClientRequest request)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new CloudScriptException("CloudScript was not called within a http request");
        }
        var userId = context.User.FindAuthUserId();

        var userData = await _userDataService.FindAsync(
            userId, userId,
            new List<string>{"TechTreeNodeData"}
        );

        var titleData = _titleDataService.Find(new List<string>{"TechTreeNodes"});
        var techTreeNodes = JsonSerializer.Deserialize<Dictionary<string, TitleDataTechTreeNodeInfo>>(titleData["TechTreeNodes"]);

        var userTechTree = JsonSerializer.Deserialize<UserTechTreeNodeData>(userData["TechTreeNodeData"].Value);
        var now = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var remainingTime = 0;
        if (userTechTree.NodeInProgress != "") {
            var nodeInProgress = userTechTree.Nodes[userTechTree.NodeInProgress];
            var node = techTreeNodes[userTechTree.NodeInProgress];
            remainingTime = nodeInProgress.UpgradeStartedTime.Seconds + node.PerkLevels[nodeInProgress.Level].UpgradeSeconds - now;
        }
        return new GetTechTreeNodeDataClientResponse
        {
            UserID = userId,
            NodesData = userTechTree.Nodes.Values.ToArray(),
            RemainingTimeInSeconds = remainingTime,
        };
    }
}
