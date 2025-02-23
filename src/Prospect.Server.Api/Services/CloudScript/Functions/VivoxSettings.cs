using Prospect.Server.Api.Services.CloudScript.Models;

namespace Prospect.Server.Api.Services.CloudScript.Functions;

[CloudScriptFunction("VivoxSettings")]
public class VivoxSettings : ICloudScriptFunction<FYEmptyRequest, FYVivoxSettingsData>
{
    public Task<FYVivoxSettingsData> ExecuteAsync(FYEmptyRequest request)
    {
        return Task.FromResult(new FYVivoxSettingsData
        {
            // TODO: Replace with app configuration
            // VivoxDomain = "",
            // VivoxIssuer = "",
            // VivoxServer = ""
        });
    }
}