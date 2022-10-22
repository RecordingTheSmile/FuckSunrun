using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FuckSunrun.Controllers
{
    [Route("console/iframes/{action}")]
    [Authorize]
    public class ConsoleIframesController:Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SunrunTasks()
        {
            return View();
        }

        public IActionResult SunrunTaskAdd()
        {
            return View();
        }
    
        [HttpGet("{id:long}")]
        public IActionResult SunrunTaskEdit()
        {
            return View();
        }

        public IActionResult SunrunTaskLogs()
        {
            return View();
        }

        public IActionResult EditMyUsername() => View();

        public IActionResult EditMyPassword() => View();

        public IActionResult EditMyEmail() => View();

        public IActionResult SunrunRunTimes() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult Users()
        {
            return View();
        }
    }
}

