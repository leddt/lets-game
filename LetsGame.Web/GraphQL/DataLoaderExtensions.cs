using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using LetsGame.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.GraphQL
{
    public static class DataLoaderExtensions
    {
        public static Task<Group> LoadGroup(this IResolverContext ctx, long id)
        {
            var loader = ctx.GetBatchDbDataLoader<long, Group>((db, keys, ct) =>
                db.Groups
                    .Where(x => keys.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, ct)
            );

            return loader.LoadAsync(id);
        }
        
        public static Task<Group> LoadGroupBySlug(this IResolverContext ctx, string slug)
        {
            var loader = ctx.GetBatchDbDataLoader<string, Group>((db, keys, ct) =>
                db.Groups
                    .Where(x => keys.Contains(x.Slug))
                    .ToDictionaryAsync(x => x.Slug, ct)
            );

            return loader.LoadAsync(slug);
        }

        public static Task<GroupGame> LoadGame(this IResolverContext ctx, long id)
        {
            var loader = ctx.GetBatchDbDataLoader<long, GroupGame>((db, keys, ct) =>
                db.GroupGames
                    .Where(x => keys.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, ct)
            );

            return loader.LoadAsync(id);
        }

        public static Task<Membership> LoadMembership(this IResolverContext ctx, long groupId, string userId)
        {
            var loader = ctx.GetBatchDbDataLoader<string, Membership>((db, keys, ct) =>
                db.Memberships
                    .Where(x => keys.Contains(x.GroupId + "/" + x.UserId))
                    .ToDictionaryAsync(x => x.GroupId + "/" + x.UserId, ct)
            );

            return loader.LoadAsync($"{groupId}/{userId}");
        }

        public static Task<GroupEvent[]> LoadEventsWithSlotsByGroupId(this IResolverContext ctx, long id)
        {
            var loader = ctx.GetGroupedDbDataLoader<long, GroupEvent>(async (db, keys, ct) =>
            {
                var result = await db.GroupEvents
                    .Include(x => x.Slots)
                    .Where(x => keys.Contains(x.GroupId))
                    .ToListAsync(ct);
                
                return result.ToLookup(x => x.GroupId);
            });

            return loader.LoadAsync(id);
        }

        public static Task<GroupGame[]> LoadGamesByGroupId(this IResolverContext ctx, long id)
        {
            var loader = ctx.GetGroupedDbDataLoader<long, GroupGame>(async (db, keys, ct) =>
            {
                var result = await db.GroupGames
                    .Where(x => keys.Contains(x.GroupId))
                    .ToListAsync(ct);

                return result.ToLookup(x => x.GroupId);
            });

            return loader.LoadAsync(id);
        }

        public static Task<GroupInvite[]> LoadInvitesByGroupId(this IResolverContext ctx, long id)
        {
            var loader = ctx.GetGroupedDbDataLoader<long, GroupInvite>(async (db, keys, ct) =>
            {
                var result = await db.GroupInvites
                    .Where(x => keys.Contains(x.GroupId))
                    .ToListAsync(ct);

                return result.ToLookup(x => x.GroupId);
            });

            return loader.LoadAsync(id);
        }

        public static Task<Membership[]> LoadMembershipsByGroupId(this IResolverContext ctx, long id)
        {
            var loader = ctx.GetGroupedDbDataLoader<long, Membership>(async (db, keys, ct) =>
            {
                var result = await db.Memberships
                    .Where(x => keys.Contains(x.GroupId))
                    .ToListAsync(ct);

                return result.ToLookup(x => x.GroupId);
            });

            return loader.LoadAsync(id);
        }

        public static Task<Membership[]> LoadMembershipsBySlotId(this IResolverContext ctx, long id)
        {
            var loader = ctx.GetGroupedDbDataLoader<long, Membership>(async (db, keys, ct) =>
            {
                var result = await db.GroupEventSlotVotes
                    .Where(x => keys.Contains(x.SlotId))
                    .Join(
                        db.Memberships, 
                        x => new { g = x.Slot.Event.GroupId, u = x.VoterId}, 
                        x => new { g = x.GroupId, u = x.UserId}, 
                        (vote, membership) => new {vote.SlotId, membership})
                    .ToListAsync(ct);

                return result.ToLookup(x => x.SlotId, x => x.membership);
            });

            return loader.LoadAsync(id);
        }

        public static Task<Membership[]> LoadMembershipsByCantPlayEventId(this IResolverContext ctx, long id)
        {
            var loader = ctx.GetGroupedDbDataLoader<long, Membership>(async (db, keys, ct) =>
            {
                var result = await db.GroupEventCantPlays
                    .Where(x => keys.Contains(x.EventId))
                    .Join(
                        db.Memberships, 
                        x => new { g = x.Event.GroupId, u = x.UserId}, 
                        x => new { g = x.GroupId, u = x.UserId}, 
                        (cantPlay, membership) => new {cantPlay.EventId, membership})
                    .ToListAsync(ct);

                return result.ToLookup(x => x.EventId, x => x.membership);
            });

            return loader.LoadAsync(id);
        }

        public static Task<string[]> LoadVoterIdsByEventId(this IResolverContext ctx, long id)
        {
            var loader = ctx.GetGroupedDbDataLoader<long, string>(async (db, keys, ct) =>
            {
                var result = await db.GroupEvents
                    .Where(x => keys.Contains(x.Id))
                    .SelectMany(x => x.Slots)
                    .SelectMany(x => x.Votes)
                    .Select(x => new { x.Slot.EventId, x.VoterId })
                    .Union(db.GroupEventCantPlays
                        .Where(x => keys.Contains(x.EventId))
                        .Select(x => new { x.EventId, VoterId = x.UserId }))
                    .Distinct()
                    .ToListAsync(ct);

                return result.ToLookup(x => x.EventId, x => x.VoterId);
            });

            return loader.LoadAsync(id);
        }

        public static Task<GroupEventSlot[]> LoadSlotsByEventId(this IResolverContext ctx, long id)
        {
            var loader = ctx.GetGroupedDbDataLoader<long, GroupEventSlot>(async (db, keys, ct) =>
            {
                var result = await db.GroupEventSlots
                    .Where(x => keys.Contains(x.EventId))
                    .ToListAsync(ct);

                return result.ToLookup(x => x.EventId);
            });

            return loader.LoadAsync(id);
        }

        
        private delegate Task<Dictionary<TKey, TValue>> DbLoadFunc<TKey, TValue>(
            ApplicationDbContext db,
            IReadOnlyList<TKey> keys,
            CancellationToken ct);
        
        private static IDataLoader<TKey, TValue> GetBatchDbDataLoader<TKey, TValue>(
            this IResolverContext ctx,
            DbLoadFunc<TKey, TValue> loadData,
            [CallerMemberName] string caller = null)
        {
            var factory = ctx.Service<IDbContextFactory<ApplicationDbContext>>();
            return ctx.BatchDataLoader<TKey, TValue>(async (keys, ct) =>
            {
                await using var db = factory.CreateDbContext();
                return await loadData(db, keys, ct);
            }, caller);
        }

        private delegate Task<ILookup<TKey, TValue>> DbGroupLoadFunc<TKey, TValue>(
            ApplicationDbContext db,
            IReadOnlyList<TKey> keys,
            CancellationToken ct);
        
        private static IDataLoader<TKey, TValue[]> GetGroupedDbDataLoader<TKey, TValue>(
            this IResolverContext ctx,
            DbGroupLoadFunc<TKey, TValue> loadData,
            [CallerMemberName] string caller = null)
        {
            var factory = ctx.Service<IDbContextFactory<ApplicationDbContext>>();
            return ctx.GroupDataLoader<TKey, TValue>(async (keys, ct) =>
            {
                await using var db = factory.CreateDbContext();
                return await loadData(db, keys, ct);
            }, caller);
        }
    }
}