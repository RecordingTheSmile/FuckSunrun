using System;
using Microsoft.AspNetCore.Mvc;

namespace FuckSunrun.Common.R
{
    public static class R
    {
        public static IActionResult Json(string message,object? data=null,int status=200)
        {
            return new JsonResult(new
            {
                message,
                data,
                status
            })
            {
                StatusCode = status
            };
        }
    }
}

