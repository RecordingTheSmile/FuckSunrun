using System;
using System.Security.Claims;

namespace FuckSunrun.Common.HttpContextExts
{
    public static class HttpContextExts
    {
        public static long GetCurrentUserId(this HttpContext context)
        {
            return Convert.ToInt64(context.User?.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value);
        }
    }
}

