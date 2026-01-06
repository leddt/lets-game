using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using LetsGame.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.GraphQL.DataLoaders;

internal static class GroupDataLoader
{
    [DataLoader]
    public static async Task<Dictionary<long, Group>> GetGroupByIdAsync(
        IReadOnlyList<long> groupIds,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        return await db.Groups
            .Where(x => groupIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
    }
    
    [DataLoader]
    public static async Task<Dictionary<string, Group>> GetGroupBySlugAsync(
        IReadOnlyList<string> slugs,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        return await db.Groups
            .Where(x => slugs.Contains(x.Slug))
            .ToDictionaryAsync(x => x.Slug, ct);
    }
}