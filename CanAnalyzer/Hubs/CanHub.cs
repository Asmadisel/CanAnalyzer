using CanAnalyzer.Models;
using Microsoft.AspNetCore.SignalR;

namespace CanAnalyzer.Hubs;

public class CanHub : Hub
{
    // Метод для отправки уведомлений
    public async Task SendNotificationAsync(StatusEvent @event, string statusName)
    {
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            SdoId = @event.Sdo,
            StatusCode = @event.StatusCode,
            StatusName = statusName,
            Time = @event.RecordedAt
        });
    }

    // Метод для обновления данных
    public async Task RefreshDataAsync()
    {
        await Clients.All.SendAsync("RefreshData");
    }
}