using System;
using System.Security.Claims;
using CDB.Captcha;
using FuckSunrun.Authentication;
using FuckSunrun.Common.HttpContextExts;
using FuckSunrun.Common.Utils;
using FuckSunrun.Models.Dtos;
using FuckSunrun.Services.Database;
using FuckSunrun.Services.MailSender;
using FuckSunrun.Services.Managers;
using FuckSunrun.Services.Sunrun;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FuckSunrun.Controllers
{
    [Route("console/api")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public class ConsoleApiController : ControllerBase
    {
        private readonly UserManager _userManager;
        private readonly Database _database;
        private readonly MailSender _mailSender;
        private readonly SunrunTaskManage _sunrunTaskManage;

        public ConsoleApiController(UserManager userManager,Database database,MailSender mailSender,SunrunTaskManage sunrunTaskManage)
        {   
            _userManager = userManager;
            _database = database;
            _mailSender = mailSender;
            _sunrunTaskManage = sunrunTaskManage;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> PostLoginAsync([FromBody]PostLoginBody body)
        {
            var userId = await _userManager.CanLoginAsync(body.Username, body.Password, HttpContext.Connection.RemoteIpAddress?.ToString());

            HttpContext.Session.SetString("userId", userId.ToString());

            if (string.IsNullOrWhiteSpace(body.Remember))
            {
                HttpContext.Response.Cookies.Delete("username", new CookieOptions { Path = "/console/login" });
            }
            else
            {
                HttpContext.Response.Cookies.Append("username", body.Username ,new CookieOptions { Path = "/console/login" });
            }

            return Ok();
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> PostRegisterAsync([FromBody]PostRegisterBody body)
        {
            var sessionCode = HttpContext.Session.GetString("emailCode");

            if (string.IsNullOrWhiteSpace(sessionCode))
            {
                return BadRequest("请先获取邮箱验证码");
            }

            if(sessionCode != body.Code)
            {
                return BadRequest("邮箱验证码不正确");
            }

            HttpContext.Session.Remove("emailCode");
            
            await _userManager.AddUserAsync(body.Username, body.Password, body.Email);

            return Ok();
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet("captcha")]
        [AllowAnonymous]
        public IActionResult GetCaptcha()
        {
            var ch = new CaptchaHelper();

            var strCode = ch.GetRandomEnDigitalText(5);

            var code = ch.GetGifEnDigitalCodeByte(strCode);

            HttpContext.Session.SetString("verifyCode", strCode.ToLower());

            return Ok(new
            {
                captcha=Convert.ToBase64String(code)
            });
        }

        /// <summary>
        /// 发送邮件验证码
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("sendEmailCode")]
        [AllowAnonymous]
        public IActionResult PostSendEmailCode([FromBody] PostSendEmailCodeBody body)
        {
            var sessionCode = HttpContext.Session.GetString("verifyCode");

            if (string.IsNullOrWhiteSpace(sessionCode))
            {
                return BadRequest("请重新获取验证码");
            }

            if (sessionCode != body.Code.ToLower().Trim())
            {
                return BadRequest("验证码错误");
            }

            HttpContext.Session.Remove("verifyCode");

            var emailCode = Utils.GenerateRandomString(8);

            _mailSender.SendEmailCodeAsync(body.Email, emailCode, "用户");

            HttpContext.Session.SetString("emailCode", emailCode);

            return Ok();
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPut("resetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> PostResetPasswordAsync([FromBody]PutResetPasswordBody body)
        {
            var sessionCode = HttpContext.Session.GetString("emailCode");

            if (string.IsNullOrWhiteSpace(sessionCode))
            {
                return BadRequest("请先获取邮箱验证码");
            }

            if(sessionCode != body.Code)
            {
                return BadRequest("邮箱验证码不正确");
            }

            HttpContext.Session.Remove("emailCode");

            var userId = await _database.Users.Where(x => x.Email == body.Email)
                .AsNoTracking().Select(x => x.Id)
                .SingleOrDefaultAsync<long>();

            if (userId == 0) return NotFound("用户不存在");

            await _userManager.ChangeUserPasswordAsync(userId, body.Password);

            return Ok();
        }

        /// <summary>
        /// 获取用户菜单
        /// </summary>
        /// <returns></returns>
        [HttpGet("menu")]
        [IgnoreAntiforgeryToken]
        public IActionResult GetMenu()
        {
            switch(User.Claims.Where(x=>x.Type == ClaimTypes.Role).FirstOrDefault()?.Value)
            {
                case "Admin":
                    return new JsonResult(new
                    []
                    {
                        new
                        {
                            id=1,
                            title="仪表盘",
                            icon="layui-icon layui-icon-console",
                            type=1,
                            href="/console/iframes/index"
                        },
                        new
                        {
                            id=3,
                            title="阳光体育任务管理",
                            icon="layui-icon layui-icon-console",
                            type=1,
                            href="/console/iframes/sunrunTasks"
                        },
                        new
                        {
                            id=4,
                            title="阳光体育跑步记录",
                            icon="layui-icon layui-icon-console",
                            type=1,
                            href="/console/iframes/sunrunTaskLogs"
                        },
                        new
                        {
                            id=5,
                            title="平台用户管理",
                            icon="layui-icon layui-icon-console",
                            type=1,
                            href="/console/iframes/users"
                        }
                    });
                default:
                    return new JsonResult(new
                    []
                    {
                        new
                        {
                            id=1,
                            title="仪表盘",
                            icon="layui-icon layui-icon-console",
                            type=1,
                            href="/console/iframes/index"
                        },
                        new
                        {
                            id=3,
                            title="阳光体育任务管理",
                            icon="layui-icon layui-icon-console",
                            type=1,
                            href="/console/iframes/sunrunTasks"
                        },
                        new
                        {
                            id=4,
                            title="阳光体育跑步记录",
                            icon="layui-icon layui-icon-console",
                            type=1,
                            href="/console/iframes/sunrunTaskLogs"
                        }
                    });
            }
        }

        /// <summary>
        /// 查询所有任务信息
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasksAsync([FromQuery] GetTasksQuery query)
        {
            var queryWhere = _database.SunrunTasks.Where(x => x.BelongTo == HttpContext.GetCurrentUserId());
            var tasks = await queryWhere
                .OrderBy(x=>x.Id)
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.IsEnable,
                    x.Length,
                    x.MaxSpeed,
                    x.MinSpeed,
                    x.Minute,
                    x.Hour,
                    x.Name,
                    x.SchoolName,
                    x.Step,
                    x.FailReason
                }).ToListAsync();

            var total = await queryWhere.CountAsync();

            return Ok(new
            {
                total,
                rows = tasks
            });
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("task/{id:long}")]
        public async Task<IActionResult> DeleteTaskAsync([FromRoute]long id)
        {
            var isTaskExists = await _database.SunrunTasks.Where(x => x.Id == id && HttpContext.GetCurrentUserId() == x.BelongTo)
                .AnyAsync();

            if (!isTaskExists) return NotFound("任务不存在");

            await _sunrunTaskManage.DeleteTaskAsync(id);

            return Ok();
        }

        /// <summary>
        /// 增加任务
        /// </summary>
        /// <returns></returns>
        [HttpPost("task")]
        public async Task<IActionResult> PostTaskAsync([FromBody] PostTaskBody body)
        {
            var sunrun = new Sunrun();
            var token = await sunrun.GetToken(body.ImeiCode);
            var userinfo = await sunrun.GetUserInfo(token);
            userinfo.Latitude = body.Latitude;
            userinfo.Longitude = body.Longitude;
            userinfo.Step = body.Step;
            await _sunrunTaskManage.AddTaskAsync(HttpContext.GetCurrentUserId(),body.ImeiCode,body.IsEnable,body.Hour,body.Minute,userinfo);

            return Ok();
        }

        /// <summary>
        /// 修改任务
        /// </summary>
        /// <returns></returns>
        [HttpPut("task/{id:long}")]
        public async Task<IActionResult> PutTaskAsync([FromRoute]long id,[FromBody]PutTaskBody body)
        {
            var taskInfo = await _database.SunrunTasks.Where(x => x.Id == id && x.BelongTo == HttpContext.GetCurrentUserId())
                .SingleOrDefaultAsync();

            if (taskInfo == null) return NotFound("任务不存在");

            if(taskInfo.ImeiCode != body.ImeiCode)
            {
                var sunrun = new Sunrun();
                if (!await sunrun.VerifyImeiCode(body.ImeiCode))
                {
                    return BadRequest("IMEICODE无效");
                }
                taskInfo.ImeiCode = body.ImeiCode;
            }

            taskInfo.Latitude = body.Latitude;
            taskInfo.Longitude = body.Longitude;
            taskInfo.Step = body.Step;

            _database.SunrunTasks.Update(taskInfo);

            await _database.SaveChangesAsync();

            if(taskInfo.Hour != body.Hour || taskInfo.Minute != body.Minute)
            {
                await _sunrunTaskManage.ChangeTaskTimeAsync(id, body.Hour, body.Minute);
            }

            await _sunrunTaskManage.ChangeTaskStatusAsync(id, body.IsEnable);

            return Ok();
        }

        /// <summary>
        /// 修改任务状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPut("task/{id:long}/enable")]
        public async Task<IActionResult> PutTaskEnableAsync([FromRoute]long id,[FromBody]PutTaskEnableBody body)
        {
            if (
                !await _database.SunrunTasks.Where(x => x.Id == id && x.BelongTo == HttpContext.GetCurrentUserId())
                    .AsNoTracking().AnyAsync()
            )
            {
                return NotFound("任务不存在");
            }

            await _sunrunTaskManage.ChangeTaskStatusAsync(id, body.IsEnable);

            return Ok();
        }

        /// <summary>
        /// 立即执行任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("task/{id:long}/executeNow")]
        public async Task<IActionResult> PatchTaskExecuteNowAsync([FromRoute]long id)
        {
            if (!await _database.SunrunTasks.Where(x => x.Id == id && x.BelongTo == HttpContext.GetCurrentUserId())
                    .AsNoTracking().AnyAsync())
            {
                return NotFound("任务不存在");
            }

            await _sunrunTaskManage.ExecuteNowAsync(id);
            
            return Ok();
        }

        /// <summary>
        /// 获取跑步任务执行次数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("task/{id:long}/runTime")]
        public async Task<IActionResult> GetTaskRunTimeAsync([FromRoute]long id)
        {
            var taskInfo = await _database.SunrunTasks.Where(x => x.Id == id).AsNoTracking().Select(x => x.ImeiCode).SingleOrDefaultAsync();

            if(taskInfo == null)
            {
                return NotFound("任务不存在");
            }

            var sunrun = new Sunrun();

            var runTimes = await sunrun.GetRunTimes(taskInfo);

            return Ok(runTimes);
        }

        /// <summary>
        /// 获取任务日志
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("taskLogs")]
        public async Task<IActionResult> GetTaskLogsAsync([FromQuery]GetTaskLogsQuery query)
        {
            var queryWhere = from logs in _database.SunrunLogs
                join tasks in _database.SunrunTasks on logs.SunrunTaskId equals tasks.Id
                where logs.BelongTo == HttpContext.GetCurrentUserId()
                orderby logs.CreateAt
                select new
                {
                    logs.Id,
                    logs.FailReason,
                    logs.IsSuccess,
                    tasks.Name,
                    logs.CreateAt
                };
                    
            var rows = await queryWhere.AsNoTracking().Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync();

            var total = await queryWhere.CountAsync();

            return Ok(new
            {
                rows,
                total
            });
        }

        /// <summary>
        /// 删除任务日志
        /// </summary>
        /// <returns></returns>
        [HttpDelete("taskLogs")]
        public async Task<IActionResult> DeleteTaskLogsAsync()
        {
            var allLogs = await _database.SunrunLogs.Where(x => x.BelongTo == HttpContext.GetCurrentUserId())
                .ToListAsync();

            _database.SunrunLogs.RemoveRange(allLogs);

            await _database.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("users")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> GetUsersAsync([FromQuery]GetUsersQuery query)
        {
            var queryWhere = _database.Users.Where(x => (string.IsNullOrWhiteSpace(query.Email) || x.Email.Contains(query.Email))
            && (string.IsNullOrWhiteSpace(query.Username) || x.Username.Contains(query.Username)))
                .Select(x=>new
                {
                    x.Id,
                    x.Username,
                    x.IsEmailVerified,
                    x.Email,
                    x.Permission,
                    x.CanLogin
                });

            var rows = await queryWhere
                .OrderBy(x=>x.Id)
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .ToListAsync();

            var total = await queryWhere.CountAsync();

            return Ok(new
            {
                rows,
                total
            });
        }

        /// <summary>
        /// 修改用户是否可以登录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPut("user/{id:long}/loginStatus")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> PutUserLoginStatusAsync([FromRoute]long id,[FromBody]PutUserLoginStatusBody body)
        {
            if (id == HttpContext.GetCurrentUserId()) return BadRequest("您无法操作自己");
            
            var user = await _database.Users.Where(x => x.Id == id).SingleOrDefaultAsync();

            if (user == null) return NotFound("用户不存在");

            user.CanLogin = body.CanLogin;

            _database.Users.Update(user);

            await _database.SaveChangesAsync();

            if (!body.CanLogin)
            {
                var allTasks = await _database.SunrunTasks.Where(x => x.BelongTo == id).AsNoTracking().Select(x=>x.Id).ToListAsync();

                foreach(var item in allTasks)
                {
                    await _sunrunTaskManage.DeleteTaskAsync(item);
                }
            }

            return Ok();
        }

        /// <summary>
        /// 修改用户邮箱验证状态
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPut("user/{id:long}/emailStatus")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> PutUserEmailStatusAsync(long id,[FromBody]PutUserEmailStatusBody body)
        {
            if (id == HttpContext.GetCurrentUserId()) return BadRequest("您无法操作自己");

            await _userManager.ChangeUserEmailStatusAsync(id, body.IsEmailVerified);

            return Ok();
        }

        /// <summary>
        /// 修改当前用户用户名
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPut("me/username")]
        public async Task<IActionResult> PutMeUsernameAsync([FromBody] PutMeUsernameBody body)
        {
            await _userManager.ChangeUserUsernameAsync(HttpContext.GetCurrentUserId(),body.Username);

            return Ok();
        }

        /// <summary>
        /// 修改当前用户密码
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPut("me/password")]
        public async Task<IActionResult> PutMePasswordAsync([FromBody] PutMePasswordBody body)
        {
            await _userManager.ChangeUserPasswordAsync(HttpContext.GetCurrentUserId(), body.Password);

            return Ok();
        }

        /// <summary>
        /// 修改当前用户邮箱
        /// </summary>
        /// <returns></returns>
        [HttpPut("me/email")]
        public async Task<IActionResult> PutMeEmailAsync([FromBody]PutMeEmailBody body)
        {
            var sessionCode = HttpContext.Session.GetString("emailCode");

            if (string.IsNullOrWhiteSpace(sessionCode))
            {
                return BadRequest("请先获取邮箱验证码");
            }

            if (sessionCode != body.Code)
            {
                return BadRequest("邮箱验证码不正确");
            }

            HttpContext.Session.Remove("emailCode");

            await _userManager.ChangeUserEmailAsync(HttpContext.GetCurrentUserId(), body.Email);

            return Ok();
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        [Route("exitLogin")]
        [IgnoreAntiforgeryToken]
        public IActionResult ExitLogin()
        {
            HttpContext.Session.Clear();

            return Redirect("/console/login");
        }
    }
}

