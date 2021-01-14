using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services.Igdb;
using LetsGame.Web.Services.Igdb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Pages.Groups
{
    public class AddGame : PageModel
    {
        [BindProperty] public string SearchText { get; set; }
        [BindProperty] public long GameToAdd { get; set; }
        
        public Game[] Results { get; set; }

        public async Task OnPostSearch([FromServices] IgdbClient client)
        {
            Results = await client.SearchGamesAsync(SearchText);
        }

        public async Task<IActionResult> OnPostAdd(
            [FromServices] ApplicationDbContext db,
            [FromServices] IgdbClient client, 
            string slug)
        {
            var group = await db.Groups.Include(x => x.Games).FirstOrDefaultAsync(x => x.Slug == slug);
            
            if (group == null) return NotFound();
            if (group.Games.Any(x => x.IgdbId == GameToAdd))
                return RedirectToPage("/Groups/Group", new {slug});

            var game = await client.GetGameAsync(GameToAdd);
            if (game == null) return NotFound();

            var groupGame = new GroupGame
            {
                Group = group,
                IgdbId = game.Id,
                Name = game.Name,
                IgdbImageId = game.MainImage?.ImageId
            };

            db.GroupGames.Add(groupGame);
            await db.SaveChangesAsync();

            return RedirectToPage("/Groups/Group", new {slug});
        }
    }
}