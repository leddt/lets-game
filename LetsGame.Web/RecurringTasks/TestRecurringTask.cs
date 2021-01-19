using System;
using System.Threading.Tasks;
using LetsGame.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace LetsGame.Web.RecurringTasks
{
    public class TestRecurringTask : IRecurringTask
    {
        private readonly ApplicationDbContext _db;

        public TestRecurringTask(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Run()
        {
            var groups = await _db.Groups.ToListAsync();
            foreach (var g in groups)
                Console.WriteLine("- {0}", g.Name);
        }
    }
}