using CanAnalyzer.Data;
using CanAnalyzer.Hubs;
using CanAnalyzer.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Timers;

namespace CanAnalyzer.Services;

public class EventGeneratorService : BackgroundService, IEventGenerator
{
    private readonly ILogger<EventGeneratorService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private System.Timers.Timer? _timer;

    // СТАТИЧЕСКОЕ поле — общее для всех экземпляров!
    private static bool _isEnabled = false;

    private readonly Random _random = new();
    private readonly int[] _sdoIds = { 1, 3, 4 };
    private readonly int[] _statusCodes = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    public EventGeneratorService(ILogger<EventGeneratorService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void Enable()
    {
        _isEnabled = true;
        _logger.LogInformation("✅ Генерация ВКЛЮЧЕНА");
    }

    public void Disable()
    {
        _isEnabled = false;
        _logger.LogInformation("⏹ Генерация ВЫКЛЮЧЕНА");
    }

    public bool IsEnabled() => _isEnabled;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Генератор событий запущен (таймер каждые 5 секунд)");
        _timer = new System.Timers.Timer(5000);
        _timer.Elapsed += async (s, e) => await GenerateEvent(stoppingToken);
        _timer.Start();
    }

    private async Task GenerateEvent(CancellationToken stoppingToken)
    {
        _logger.LogInformation("⏰ Таймер сработал. _isEnabled = {_isEnabled}", _isEnabled);

        if (!_isEnabled)
        {
            _logger.LogInformation("⏭ Пропускаем генерацию");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var sdoId = _sdoIds[_random.Next(_sdoIds.Length)];
            var statusCode = _statusCodes[_random.Next(_statusCodes.Length)];
            var currentTime = DateTime.UtcNow;

            var statusName = await context.Status
                .Where(s => s.StatusCodeId == statusCode)
                .Select(s => s.StatusName)
                .FirstOrDefaultAsync(stoppingToken) ?? "Неизвестный статус";

            var newEvent = new StatusEvent
            {
                Time = currentTime,
                StatusCode = statusCode,
                Sdo = sdoId,
                RecordedAt = currentTime
            };

            context.StatusEvent.Add(newEvent);
            await context.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("✅ Сгенерировано событие: SDO={Sdo}, StatusCode={StatusCode}, StatusName={StatusName}",
                sdoId, statusCode, statusName);

            // Отправляем уведомление через SignalR
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<CanHub>>();

            _logger.LogInformation("📡 Отправляем уведомление через SignalR...");
            await hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                SdoId = newEvent.Sdo,
                StatusCode = newEvent.StatusCode,
                StatusName = statusName,
                Time = newEvent.RecordedAt
            });

            _logger.LogInformation("✅ Уведомление отправлено");

            // Обновляем данные на странице
            await hubContext.Clients.All.SendAsync("RefreshData");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка генерации события");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Stop();
        await base.StopAsync(stoppingToken);
        _logger.LogInformation("🛑 Генератор событий остановлен");
    }
}