using System.Threading.Tasks;
using LetsGame.Web.RecurringTasks;
using Microsoft.AspNetCore.Mvc;

namespace LetsGame.Web.Controllers;

public class RecurringTasksController : Controller
{
    private readonly RecurringTaskRunner _runner;

    public RecurringTasksController(RecurringTaskRunner runner)
    {
        _runner = runner;
    }

    [HttpPost("run-recurring-tasks")]
    public async Task<IActionResult> RunTasks()
    {
        await _runner.RunAll();
        return NoContent();
    }
}