﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Hubs;
using LetsGame.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Pages.Groups
{
    [Authorize]
    public class GroupModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly GroupService _groupService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<GroupHub> _hubContext;

        public GroupModel(
            ApplicationDbContext db, 
            UserManager<AppUser> userManager, 
            GroupService groupService,
            INotificationService notificationService,
            IHubContext<GroupHub> hubContext)
        {
            _db = db;
            _userManager = userManager;
            _groupService = groupService;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        public Group Group { get; set; }
        public Membership UserMembership { get; set; }
        public string IcalLink { get; set; }
        public IEnumerable<GroupEvent> ProposedEvents => Group.Events.Where(x => x.ChosenDateAndTimeUtc == null);
        public IEnumerable<GroupEvent> UpcomingEvents => Group.Events.Where(x => x.ChosenDateAndTimeUtc.HasValue).OrderBy(x => x.ChosenDateAndTimeUtc);
        
        [BindProperty] public long EventId { get; set; }
        [BindProperty] public long SlotId { get; set; }
        [BindProperty] public long GameId { get; set; }
        [BindProperty] public bool SingleUse { get; set; }
        [BindProperty] public string InviteId { get; set; }
        [BindProperty] public string MemberId { get; set; }
        [BindProperty] public int AvailabilityLength { get; set; }
        
        public string GetDisplayName(string userId) =>
            Group.Memberships.FirstOrDefault(x => x.UserId == userId)?.DisplayName ?? "Unknown member";

        public bool IsGroupOwner => UserMembership.Role == GroupRole.Owner;
        public string UserId => _userManager.GetUserId(User);
        
        public async Task<IActionResult> OnGetAsync(string slug)
        {
            var utcThreshold = DateTime.UtcNow - TimeSpan.FromHours(6);
            
            Group = await _db.Groups
                .TagWith("Load group page data")
                .AsSplitQuery()
                .Include(x => x.Memberships.OrderBy(m => m.Role == GroupRole.Owner ? 0 : 1).ThenBy(m => m.DisplayName))
                .Include(x => x.Games.OrderBy(g => g.Name))
                .Include(x => x.Events
                    .Where(e => e.ChosenDateAndTimeUtc > utcThreshold || 
                                e.ChosenDateAndTimeUtc == null && e.Slots.Any(s => s.ProposedDateAndTimeUtc > utcThreshold)))
                    .ThenInclude(x => x.Game)
                .Include(x => x.Events)
                    .ThenInclude(x => x.Slots.Where(s => s.ProposedDateAndTimeUtc > utcThreshold).OrderBy(s => s.ProposedDateAndTimeUtc))
                    .ThenInclude(x => x.Votes.OrderBy(v => v.VotedAtUtc))
                .Include(x => x.Events)
                    .ThenInclude(x => x.CantPlays.OrderBy(c => c.AddedAtUtc))
                .Include(x => x.Invites.OrderBy(i => i.CreatedAtUtc))
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync(x => x.Slug == slug);

            if (string.IsNullOrWhiteSpace(Group.SharingKey))
            {
                Group.SharingKey = Guid.NewGuid().ToString("N");
                await _db.SaveChangesAsync();
            }
            
            IcalLink = Url.Action("GetGroupCalendarAsIcal", "Ical", new {slug, k = Group.SharingKey}, Request.Scheme);
            
            UserMembership = Group?.Memberships.FirstOrDefault(x => x.UserId == UserId);

            if (Group == null) return NotFound();
            if (UserMembership == null) return NotFound();
            
            return Page();
        }

        public async Task<IActionResult> OnPostDelete(string slug)
        {
            var group = await _db.Groups
                .Include(x => x.Events)
                .FirstOrDefaultAsync(x =>
                    x.Slug == slug &&
                    x.Memberships.Any(m => m.Role == GroupRole.Owner &&
                                           m.User.Id == UserId));

            if (group != null)
            {
                if (group.Events.Any()) 
                    _db.GroupEvents.RemoveRange(group.Events);
                
                _db.Groups.Remove(group);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostVoteSlot(string slug)
        {
            await _groupService.AddSlotVoteAsync(SlotId);
            NotifyClients(slug, $"event-{EventId}");

            return RedirectToPage("Group", new {slug});
        }

        public async Task<IActionResult> OnPostUnvoteSlot(string slug)
        {
            await _groupService.RemoveSlotVoteAsync(SlotId);
            NotifyClients(slug, $"event-{EventId}");

            return RedirectToPage("Group", new {slug});
        }

        public async Task<IActionResult> OnPostCantPlay(string slug)
        {
            await _groupService.SetCantPlayAsync(EventId);
            NotifyClients(slug, $"event-{EventId}");

            return RedirectToPage("Group", new {slug});
        }

        public async Task<IActionResult> OnPostPickSlot(string slug)
        {
            await _groupService.PickSlotAsync(SlotId);
            NotifyClients(slug, $"event-{EventId}");
            
            return RedirectToPage("Group", new {slug});
        }

        public async Task<IActionResult> OnPostCreateInvite(string slug)
        {
            var group = await _groupService.FindBySlugAsync(slug);
            
            if (group != null)
            {
                await _groupService.CreateInviteAsync(group.Id, SingleUse);
            }

            return RedirectToPage("Group", new {slug});
        }

        public async Task<IActionResult> OnPostDeleteInvite(string slug)
        {
            await _groupService.DeleteInviteAsync(InviteId);

            return RedirectToPage("Group", new {slug});
        }

        public async Task<IActionResult> OnPostDeleteMember(string slug)
        {
            var group = await _groupService.FindBySlugAsync(slug);
            await _groupService.RemoveGroupMember(group.Id, MemberId);

            return RedirectToPage("Group", new {slug});
        }

        public async Task<IActionResult> OnPostLeaveGroup(string slug)
        {
            var group = await _groupService.FindBySlugAsync(slug);
            await _groupService.LeaveGroup(group.Id);

            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostRemoveGame(string slug)
        {
            var game = await _db.GroupGames
                .Where(x => x.Group.Memberships.Any(m => m.UserId == UserId && m.Role == GroupRole.Owner))
                .FirstOrDefaultAsync(x => x.Group.Slug == slug && x.Id == GameId);
            
            if (game != null)
            {
                _db.GroupGames.Remove(game);
                await _db.SaveChangesAsync();
            }

            return RedirectToPage("Group", new {slug});
        }

        public async Task<IActionResult> OnPostDeleteEvent(string slug)
        {
            var ev = await _db.GroupEvents
                .Where(x => x.CreatorId == UserId ||
                            x.Group.Memberships.Any(m => m.Role == GroupRole.Owner && m.UserId == UserId))
                .FirstOrDefaultAsync(x => x.Id == EventId);

            if (ev != null)
            {
                _db.GroupEvents.Remove(ev);
                await _db.SaveChangesAsync();
            }
            
            return RedirectToPage("Group", new {slug});
        }

        public async Task<IActionResult> OnPostRemind(string slug)
        {
            await _notificationService.SendEventReminderAsync(EventId);
            NotifyClients(slug, $"event-{EventId}");
            
            return RedirectToPage("Group", new {slug});
        }

        public async Task<IActionResult> OnPostSetAvailability(string slug)
        {
            var member = await _db.Memberships
                .Include(x => x.Group)
                .Where(x => x.Group.Slug == slug)
                .Where(x => x.UserId == UserId)
                .FirstOrDefaultAsync();

            if (member != null)
            {
                var wasAvailable = member.IsAvailableNow();

                var utcNow = DateTime.UtcNow;
                member.AvailableUntilUtc = utcNow + TimeSpan.FromSeconds(AvailabilityLength);
                await _db.SaveChangesAsync();

                if (member.IsAvailableNow() &&
                    !wasAvailable &&
                    (member.AvailabilityNotificationSentAtUtc == null ||
                     member.AvailabilityNotificationSentAtUtc < utcNow - TimeSpan.FromHours(1)))
                {
                    member.AvailabilityNotificationSentAtUtc = utcNow;
                    await _db.SaveChangesAsync();

                    await _notificationService.NotifyMemberAvailable(member.Group, member);
                }

                NotifyClients(slug, "members");
            }

            return RedirectToPage("Group", new {slug});
        }

        private void NotifyClients(string slug, string elementId)
        {
            _hubContext.Clients.Group(slug).SendAsync("update", elementId);
        }
    }
}