using System.Text.Json.Serialization;
using Prospect.Server.Api.Services.CloudScript;

public class ReportBiEventsRequest
{
    [JsonPropertyName("messages")]
    public object[] Messages { get; set; }
}

public class ReportBiEventsResponse
{
    // TODO: Unknown structure
}

[CloudScriptFunction("ReportBiEvents")]
public class ReportBiEventsFunction : ICloudScriptFunction<ReportBiEventsRequest, ReportBiEventsResponse>
{
    private readonly ILogger<ReportBiEventsFunction> _logger;

    public ReportBiEventsFunction(ILogger<ReportBiEventsFunction> logger)
    {
        _logger = logger;
    }

    public async Task<ReportBiEventsResponse> ExecuteAsync(ReportBiEventsRequest request)
    {
        _logger.LogInformation("Processing Bi Event");

        return new ReportBiEventsResponse{};
    }
}
