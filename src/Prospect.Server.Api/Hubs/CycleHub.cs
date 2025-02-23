using Microsoft.AspNetCore.SignalR;

namespace Prospect.Server.Api.Hubs;

public class CycleHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("Connected {0}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
}