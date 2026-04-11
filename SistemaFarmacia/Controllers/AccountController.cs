using Microsoft.AspNetCore.Mvc;
using SistemaFarmacia.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

public class AccountController : Controller
{
    private readonly FarmaciaContext _context;

    public AccountController(FarmaciaContext context)
    {
        _context = context;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var user = _context.Usuarios
            .FirstOrDefault(u => u.Username == username && u.Password == password);

        if (user != null)
        {
            HttpContext.Session.SetString("Usuario", user.Username);
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Usuario o contraseña incorrectos";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}