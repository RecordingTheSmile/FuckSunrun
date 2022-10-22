using System;
using System.Security.Claims;
using Flurl;
using FuckSunrun.Services.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace FuckSunrun.Authentication
{
    public class UserAuthentication:IAuthenticationHandler
    {
        public const string SchemeName = "UserAuthentication";

        private HttpContext _context;

        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            var userId = _context.Session.GetString("userId");

            if (string.IsNullOrWhiteSpace(userId))
            {
                return AuthenticateResult.Fail(string.Empty);
            }

            var userIdLong = Convert.ToInt64(userId);

            await using var db = new Database();

            var userInfo = await db.Users.Where(x => x.Id == userIdLong).AsNoTracking().Select(x=>new
            {
                x.Username,
                x.Permission,
                x.CanLogin,
                x.IsEmailVerified
            }).FirstOrDefaultAsync();

            if(userInfo == null)
            {
                return AuthenticateResult.Fail(string.Empty);
            }

            if(!userInfo.CanLogin || !userInfo.IsEmailVerified)
            {
                return AuthenticateResult.Fail(string.Empty);
            }

            var principle = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name,userInfo.Username),
                new Claim(ClaimTypes.NameIdentifier,userId),
                new Claim(ClaimTypes.Role,userInfo.Permission.ToString())
            },SchemeName));

            var ticket = new AuthenticationTicket(principle,SchemeName);

            return AuthenticateResult.Success(ticket);
        }

        public async Task ChallengeAsync(AuthenticationProperties? properties)
        {
            _context.Response.StatusCode = 401;
            if(_context.Request.Headers["X-Requested-With"].FirstOrDefault() == "XMLHttpRequest")
            {
                await _context.Response.WriteAsJsonAsync(new
                {
                    message="登录信息过期，请重新登录",
                    status=401
                });
            }
            else
            {
                _context.Response.Redirect("/console/login".SetQueryParam("to",_context.Request.Path+"?"+_context.Request.QueryString).SetQueryParam("loginFail",1));
            }
        }

        public async Task ForbidAsync(AuthenticationProperties? properties)
        {
            _context.Response.StatusCode = 403;
            if (_context.Request.Headers["X-Requested-With"].FirstOrDefault() == "XMLHttpRequest")
            {
                await _context.Response.WriteAsJsonAsync(new
                {
                    message = "您无权访问此页面",
                    status=403
                });
            }
            else
            {
                _context.Response.Redirect("/console/login".SetQueryParam("to", _context.Request.Path + "?" + _context.Request.QueryString).SetQueryParam("loginFail", 1));
            }
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            _context = context;
            return Task.CompletedTask;
        }
    }
}

