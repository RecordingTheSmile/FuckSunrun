using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FuckSunrun.Controllers
{
    public class ConsoleController:Controller
    {
        
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return Redirect("/console/index");
            else
                return View();
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Forbidden()
        {
            ViewData["code"] = 403;
            ViewData["message"] = "您无权查看此页面";
            return View("Error");
        }

        public IActionResult ResetPassword()
        {
            return View();
        }
    }
}

