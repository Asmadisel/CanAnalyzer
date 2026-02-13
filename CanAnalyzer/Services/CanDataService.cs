using CanAnalyzer.Data;
using CanAnalyzer.Models;
using Microsoft.EntityFrameworkCore;

namespace CanAnalyzer.Services;

public class CanDataService
{
    private readonly ApplicationDbContext _context;

    public CanDataService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SdoWithStats>> GetSdoListAsync()
    {
        //  Получаем статистику по событиям
        var stats = await _context.StatusEvent
            .GroupBy(se => se.Sdo)
            .Select(g => new {
                SdoId = g.Key,
                Count = g.Count(),
                Latest = g.Max(se => se.RecordedAt)
            })
            .OrderByDescending(x => x.Latest)
            .Take(50)
            .ToListAsync();

        // Получаем все SDO с типами
        var sdoIds = stats.Select(x => x.SdoId).ToList();
        var sdoDict = await _context.Sdo
            .Include(s => s.SdoTypeNavigation)
            .Where(s => sdoIds.Contains(s.SdoId))
            .ToDictionaryAsync(s => s.SdoId);

        // Объединяем данные
        var result = new List<SdoWithStats>();
        foreach (var stat in stats)
        {
            if (sdoDict.TryGetValue(stat.SdoId, out var sdo))
            {
                result.Add(new SdoWithStats
                {
                    Sdo = sdo,
                    TotalCount = stat.Count,
                    LatestRecordedAt = stat.Latest
                });
            }
        }

        return result;
    }

    public async Task<List<StatusStats>> GetStatusStatsForSdoAsync(int sdoId)
    {
        return await _context.StatusEvent
            .Where(se => se.Sdo == sdoId)
            .Include(se => se.StatusNavigation)
            .GroupBy(se => se.StatusNavigation)
            .Select(g => new StatusStats
            {
                Status = g.Key,
                Count = g.Count(),
                LatestRecordedAt = g.Max(se => se.RecordedAt)
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
    }

    public class SdoWithStats
    {
        public Sdo Sdo { get; set; } = null!;
        public int TotalCount { get; set; }
        public DateTime LatestRecordedAt { get; set; }
    }

    public class StatusStats
    {
        public Status Status { get; set; } = null!;
        public int Count { get; set; }
        public DateTime LatestRecordedAt { get; set; }
    }
}