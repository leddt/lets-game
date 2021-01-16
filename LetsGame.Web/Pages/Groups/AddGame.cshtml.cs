using System.Linq;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Services;
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
            [FromServices] GroupService groupService,
            string slug)
        {
            var group = await groupService.FindBySlugAsync(slug);
            if (group == null) return NotFound();

            await groupService.AddGameToGroupAsync(group.Id, GameToAdd);

            return RedirectToPage("/Groups/Group", new {slug});
        }
    }
}