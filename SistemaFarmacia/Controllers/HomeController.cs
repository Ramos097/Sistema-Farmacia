using Microsoft.AspNetCore.Mvc;
using SistemaFarmacia.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SistemaFarmacia.Controllers
{
    public class HomeController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                context.Result = RedirectToAction("Login", "Account");
            }
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
