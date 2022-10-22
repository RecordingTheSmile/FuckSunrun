using System;
using FuckSunrun.Common.R;
using Microsoft.AspNetCore.Mvc;

namespace FuckSunrun.Controllers
{
    public class PublicController:Controller
    {
        public IActionResult HandleNotFound()
        {
            if (Request.Headers["X-Requested-With"].FirstOrDefault() == "XMLHttpRequest")
            {
                return R.Json("页面不存在",status:404);
            }
            ViewData["code"] = 404;
            ViewData["message"] = "页面不存在";
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}

