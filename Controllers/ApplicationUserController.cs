using Microsoft.AspNetCore.Mvc;

namespace Workify_Full.Controllers
{
    public class ApplicationUserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
