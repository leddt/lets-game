using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using LetsGame.Web.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.Services
{
    public class GroupService
    {
        private readonly ApplicationDbContext _db;
        private readonly SlugGenerator _slugGenerator;

        public GroupService(ApplicationDbContext db, SlugGenerator slugGenerator)
        {
            _db = db;
            _slugGenerator = slugGenerator;
        }

        public Task<string> GetSlugFromGroupName(string name)
        {
            return _slugGenerator.GenerateWithCheck(name, slug => _db.Groups.AnyAsync(x => x.Slug == slug));
        }
    }

    public class SlugGenerator
    {
        private Random random;

        public SlugGenerator()
        {
            random = new Random();
        }

        public async Task<string> GenerateWithCheck(string name, Func<string, Task<bool>> isInUse)
        {
            const int baseSuffixLength = 3;
            const int attemptsThreshold = 5;
            
            var suffix = "";
            var attempts = 0;
            
            while (true)
            {
                var slug = (name + suffix).Sluggify();
                if (!await isInUse(slug)) return slug;
                
                suffix = GetSuffix(baseSuffixLength + attempts/attemptsThreshold);
                attempts++;
            }
        }

        private string GetSuffix(int length)
        {
            const string suffixChars = "abcdefghijklmnopqrstuvwxyz1234567890";

            var sb = new StringBuilder("-", length + 1);
            for (var i = 0; i < length; i++)
            {
                sb.Append(suffixChars[random.Next(suffixChars.Length)]);
            }

            return sb.ToString();
        }
    }
}