using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FuckSunrun.Models;
using FuckSunrun.Exceptions;

namespace FuckSunrun.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return Redirect("/console/index");
    }
}

