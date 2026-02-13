using CanAnalyzer.Components;
using CanAnalyzer.Data;
using CanAnalyzer.Hubs;
using CanAnalyzer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//  БД
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    )
);

// Регистрируем сервисы
builder.Services.AddScoped<CanDataService>();
builder.Services.AddSingleton<IEventGenerator, EventGeneratorService>();
builder.Services.AddHostedService<EventGeneratorService>();

// Добавляем SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(10);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
});

// Добавляем сервисы
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Проверка подключения к БД
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        if (!dbContext.Database.CanConnect())
        {
            throw new Exception("Не удалось подключиться к базе данных!");
        }

        var count = dbContext.Sdo.Count();
        Console.WriteLine($"✅ Подключение к БД успешно. Записей в SDO: {count}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка подключения к БД: {ex.Message}");
    }
}

// Настройка конвейера запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Маршрут для SignalR
app.MapHub<CanHub>("/canhub");

app.Run();