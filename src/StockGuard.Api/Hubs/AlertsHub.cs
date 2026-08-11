using Microsoft.AspNetCore.SignalR;

namespace StockGuard.Api.Hubs;

public class AlertsHub : Hub
{
    public async Task BroadcastAlert(string message)
    {
        await Clients.All.SendAsync("ReceiveAlert", message);
    }
}