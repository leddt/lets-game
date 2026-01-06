using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using LetsGame.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.GraphQL.DataLoaders;

internal static class GroupInviteDataLoader
{
    [DataLoader]
    public static async Task<Dictionary<long, GroupInvite[]>> GetGroupInvitesByGroupIdAsync(
        IReadOnlyList<long> keys,
        ApplicationDbContext db,
        CancellationToken ct
    ) => await db.GroupInvites
        .Where(x => keys.Contains(x.GroupId))
        .GroupBy(x => x.GroupId)
        .Select(x => new { x.Key, Items = x.ToArray()})
        .ToDictionaryAsync(x => x.Key, x => x.Items, ct);
}