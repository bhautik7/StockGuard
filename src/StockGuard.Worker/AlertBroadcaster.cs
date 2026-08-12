using Microsoft.AspNetCore.SignalR.Client;

namespace StockGuard.Worker;

public class AlertBroadcaster : IAsyncDisposable
{
    private readonly HubConnection _connection;

    public AlertBroadcaster(string hubUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task EnsureConnectedAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected)
            await _connection.StartAsync();
    }

    public async Task BroadcastAlertAsync(string message)
    {
        await EnsureConnectedAsync();
        await _connection.InvokeAsync("BroadcastAlert", message);
    }

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}