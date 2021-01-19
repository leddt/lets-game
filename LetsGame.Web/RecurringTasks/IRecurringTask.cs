using System.Threading.Tasks;

namespace LetsGame.Web.RecurringTasks
{
    public interface IRecurringTask
    {
        public Task Run();
    }
}