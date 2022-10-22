using System;
using FuckSunrun.Common.Conf;
using FuckSunrun.Common.DI;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using MailKit.Net.Smtp;
using MimeKit;

namespace FuckSunrun.Services.MailSender
{
    public class MailSender
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="address">收件人邮箱</param>
        /// <param name="title">标题</param>
        /// <param name="viewName">视图名称</param>
        /// <param name="viewData">视图数据，可空</param>
        /// <param name="nickname">用户昵称，可空，默认为空字符串</param>
        /// <returns></returns>
        [AutomaticRetry(Attempts =3,OnAttemptsExceeded =AttemptsExceededAction.Delete)]
        [MailSenderLogger]
        public async Task SendMailAsync(string address, string title ,string viewName,Dictionary<string,object>? viewData=null, string? nickname = null)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(Conf.Root["Smtp:Nickname"], Conf.Root["Smtp:Username"]));

            message.To.Add(new MailboxAddress(nickname ?? String.Empty, address));

            message.Subject = title;

            var html = await Razor.Templating.Core.RazorTemplateEngine.RenderAsync<object>($"~/Views/Email/{viewName}.cshtml", null!, viewData??new Dictionary<string, object>());

            var body = new BodyBuilder
            {
                HtmlBody = html
            };

            message.Body = body.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(Conf.Root["Smtp:Host"], Conf.Root.GetSection("Smtp:Port").Get<int>());

            await client.AuthenticateAsync(Conf.Root["Smtp:Username"], Conf.Root["Smtp:Password"]);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }

        public void SendEmailCodeAsync(string address,string code,string? nickname = null)
        {
            BackgroundJob.Enqueue(() => SendMailAsync(address, "您的邮箱验证码", "EmailCode", new Dictionary<string, object>
            {
                {"code",code }
            }, nickname));
        }

        public void SendRunResultAsync(string address,bool isSuccess,string? reason=null,string? nickname=null)
        {
            BackgroundJob.Enqueue(() => SendMailAsync(address, $"跑步任务{(isSuccess?"成功":"失败")}通知", "RunResult", new Dictionary<string, object>
            {
                {"success",isSuccess },
                {"reason",reason??"" }
            }, nickname));
        }
    }

    public class MailSenderLogger : JobFilterAttribute, IElectStateFilter
    {
        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
            {
                var logger = DI.Services.GetRequiredService<Logger<MailSender>>();

                logger.LogError(failedState.Exception, "发送邮件时出现错误");
            }
        }
    }

    public static class InjectMailSender
    {
        public static IServiceCollection AddMailSender(this IServiceCollection services)
        {
            services.AddSingleton<MailSender>();

            return services;
        }
    }
}

