using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LetsGame.Web.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public IList<Group> Groups { get; set; }
        
        public IndexModel(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task OnGet()
        {
            var userId = _userManager.GetUserId(User);
            Groups = await _db.Groups
                .Where(x => x.Memberships.Any(m => m.UserId == userId))
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}