using Flurl.Http;
using FuckSunrun.Authentication;
using FuckSunrun.Common.Conf;
using FuckSunrun.Common.DI;
using FuckSunrun.Common.HttpClientFactory;
using FuckSunrun.Filters;
using FuckSunrun.Models.Enums;
using FuckSunrun.Services.Database;
using FuckSunrun.Services.MailSender;
using FuckSunrun.Services.Managers;
using FuckSunrun.Services.Schedule;
using Hangfire;
using Hangfire.Redis;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

//全局配置Flurl
FlurlHttp.Configure(opts =>
{
    opts.HttpClientFactory = new MyHttpClientFactory();
});

var builder = WebApplication.CreateBuilder(args).AttachConfig();

//配置Hangfire
GlobalConfiguration.Configuration.UseRedisStorage(builder.Configuration.GetConnectionString("Redis"),new RedisStorageOptions
{
    Prefix = "HangfireTimer-"
});

//反CSRF攻击
builder.Services.AddAntiforgery(opts =>
{
    opts.Cookie.Name = "Security-Token";
    opts.HeaderName = "X-CSRF-TOKEN";
});

//Controller和JSON解析
builder.Services.AddControllersWithViews(opts =>
{
    opts.Filters.Add<ExceptionFilter>();
    opts.Filters.Add<ActionFilter>();
}).AddNewtonsoftJson();

//自定义用户管理
builder.Services.AddUserManager();

//自定义邮件发送类
builder.Services.AddMailSender();

//自定义阳光体育任务管理器
builder.Services.AddSunrunTaskManager();

//配置ForwardedFor头
builder.Services.Configure<ForwardedHeadersOptions>(opts =>
{
    opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    opts.KnownNetworks.Clear();
    opts.KnownProxies.Clear();
});

//分布式缓存
builder.Services.AddStackExchangeRedisCache(opts =>
{
    opts.Configuration = builder.Configuration.GetConnectionString("Redis");
    opts.InstanceName = "AspNetCoreCache";
});

//会话Session
builder.Services.AddSession(opts =>
{
    opts.Cookie.Name = "UserId";
    opts.IdleTimeout = TimeSpan.FromMinutes(20);
});

//数据库
builder.Services.AddDbContext<Database>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("PgSQL"));
});

//用户鉴权
builder.Services.AddAuthenticationCore(opts =>
{
    opts.AddScheme<UserAuthentication>(UserAuthentication.SchemeName, String.Empty);
    opts.DefaultScheme = UserAuthentication.SchemeName;
});

//添加Hangfire服务器
builder.Services.AddHostedService<HangfireBackgroundJob>();

var app = builder.Build().AttachDI();

//配置HTTP管道

app.UseStaticFiles();

//使用ForwardedFor头
app.UseForwardedHeaders();

app.UseSession();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapFallbackToController("HandleNotFound", "Public");

//初始化数据库并添加种子数据
await using(var scoped = app.Services.CreateAsyncScope())
{
    await using(var db = scoped.ServiceProvider.GetRequiredService<Database>())
    {
        await db.Database.MigrateAsync();

        if(!await db.Users.AnyAsync())
        {
            var userManager = scoped.ServiceProvider.GetRequiredService<UserManager>();

            var userId = await userManager.AddUserAsync("admin", "admin123", "admin@admin.com" ,UserPermission.Admin);

            await userManager.ChangeUserEmailStatusAsync(userId, true);
        }
    }
}

app.Run();

