using System;
using System.Text;
using System.Threading.Tasks;
using LetsGame.Web.Extensions;
using LetsGame.Web.Helpers;

namespace LetsGame.Web.Services
{
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
                
                suffix = "-" + RandomHelper.GetRandomIdentifier(baseSuffixLength + attempts/attemptsThreshold);
                attempts++;
            }
        }
    }
}