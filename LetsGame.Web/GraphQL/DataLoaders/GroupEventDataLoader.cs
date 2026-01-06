using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using LetsGame.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.GraphQL.DataLoaders;

internal static class GroupEventDataLoader
{
    [DataLoader]
    public static async Task<Dictionary<long, GroupEvent>> GetGroupEventByIdAsync(
        IReadOnlyList<long> keys,
        ApplicationDbContext db,
        CancellationToken ct
    ) => await db.GroupEvents
        .Where(x => keys.Contains(x.Id))
        .ToDictionaryAsync(x => x.Id, ct);
    
    [DataLoader]
    public static async Task<Dictionary<long, GroupEvent[]>> GetGroupEventWithSlotsByGroupIdAsync(
        IReadOnlyList<long> keys,
        ApplicationDbContext db,
        CancellationToken ct
    ) => await db.GroupEvents
        .Include(x => x.Slots)
        .Where(x => keys.Contains(x.GroupId))
        .GroupBy(x => x.GroupId)
        .Select(x => new { x.Key, Items = x.ToArray()})
        .ToDictionaryAsync(x => x.Key, x => x.Items, ct);

    [DataLoader]
    public static async Task<Dictionary<long, string[]>> GetVoterIdsByEventIdAsync(
        IReadOnlyList<long> keys,
        ApplicationDbContext db,
        CancellationToken ct
    )
    {
        var votes = await db.GroupEvents
            .Where(x => keys.Contains(x.Id))
            .SelectMany(x => x.Slots)
            .SelectMany(x => x.Votes)
            .Select(x => new { x.Slot.EventId, x.VoterId })
            .Distinct()
            .ToListAsync(ct);

        var cantPlays = await db.GroupEventCantPlays
            .Where(x => keys.Contains(x.EventId))
            .Select(x => new { x.EventId, VoterId = x.UserId })
            .ToListAsync(ct);
        
        return votes.Union(cantPlays)
            .GroupBy(x => x.EventId)
            .Select(x => new { x.Key, Items = x.Select(y => y.VoterId).ToArray() })
            .ToDictionary(x => x.Key, x => x.Items);
    }

    [DataLoader]
    public static async Task<Dictionary<long, GroupEventSlot[]>> GetSlotsByEventIdAsync(
        IReadOnlyList<long> keys,
        ApplicationDbContext db,
        CancellationToken ct
    ) => await db.GroupEventSlots
        .Where(x => keys.Contains(x.EventId))
        .GroupBy(x => x.EventId)
        .Select(x => new { x.Key, Items = x.ToArray() })
        .ToDictionaryAsync(x => x.Key, x => x.Items, ct);
}