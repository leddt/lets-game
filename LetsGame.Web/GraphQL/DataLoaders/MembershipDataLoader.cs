using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using LetsGame.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.GraphQL.DataLoaders;

internal static class MembershipDataLoader
{
    [DataLoader]
    public static async Task<Dictionary<long, Membership[]>> GetMembershipsByGroupId(
        IReadOnlyList<long> keys,
        ApplicationDbContext db,
        CancellationToken ct
    ) => await db.Memberships
        .Where(x => keys.Contains(x.GroupId))
        .GroupBy(x => x.GroupId)
        .Select(x => new { x.Key, Items = x.ToArray() })
        .ToDictionaryAsync(x => x.Key, x => x.Items, ct);

    [DataLoader]
    public static async Task<Dictionary<(long groupId, string userId), Membership>> GetMembershipByGroupIdAndUserIdAsync(
        IReadOnlyList<(long groupId, string userId)> keys,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var mappedKeys = keys.Select(x => $"{x.groupId}/{x.userId}").ToArray();
        return await db.Memberships
            .Where(x => mappedKeys.Contains(x.GroupId + "/" + x.UserId))
            .ToDictionaryAsync(x => (x.GroupId, x.UserId), ct);
    }

    [DataLoader]
    public static async Task<Dictionary<long, Membership[]>> GetMembershipsBySlotId(
        IReadOnlyList<long> keys,
        ApplicationDbContext db,
        CancellationToken ct
    ) => await db.GroupEventSlotVotes
        .Where(x => keys.Contains(x.SlotId))
        .Join(
            db.Memberships,
            x => new { g = x.Slot.Event.GroupId, u = x.VoterId },
            x => new { g = x.GroupId, u = x.UserId },
            (vote, membership) => new { vote.SlotId, membership })
        .GroupBy(x => x.SlotId, x => x.membership)
        .Select(x => new { x.Key, Items = x.ToArray() })
        .ToDictionaryAsync(x => x.Key, x => x.Items, ct);

    [DataLoader]
    public static async Task<Dictionary<long, Membership[]>> GetMembershipsByCantPlayEventId(
        IReadOnlyList<long> keys,
        ApplicationDbContext db,
        CancellationToken ct
    ) => await db.GroupEventCantPlays
        .Where(x => keys.Contains(x.EventId))
        .Join(
            db.Memberships,
            x => new { g = x.Event.GroupId, u = x.UserId },
            x => new { g = x.GroupId, u = x.UserId },
            (cantPlay, membership) => new { cantPlay.EventId, membership })
        .GroupBy(x => x.EventId, x => x.membership)
        .Select(x => new { x.Key, Items = x.ToArray() })
        .ToDictionaryAsync(x => x.Key, x => x.Items, ct);
}