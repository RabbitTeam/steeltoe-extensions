using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Time_UI.Services;

namespace Time_UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITimeService _timeService;

        public HomeController(ITimeService timeService)
        {
            _timeService = timeService;
        }

        public async Task<IActionResult> Index()
        {
            var now = await _timeService.GetNowAsync();
            return View(now);
        }
    }
}