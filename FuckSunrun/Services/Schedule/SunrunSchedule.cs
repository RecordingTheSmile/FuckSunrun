using System;
using Flurl.Http;
using FuckSunrun.Common.DI;
using FuckSunrun.Exceptions;
using FuckSunrun.Models.Entities;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.EntityFrameworkCore;

namespace FuckSunrun.Services.Schedule
{
    public static class SunrunSchedule
    {
        [AutomaticRetry(Attempts =0,OnAttemptsExceeded =AttemptsExceededAction.Delete)]
        [SunrunScheduleLogger]
        public static async Task RunAsync(long taskId,int retryTimes = 0)
        {
            await using var db = new Database.Database();

            var taskInfo = await db.SunrunTasks.Where(x => x.Id == taskId).SingleOrDefaultAsync();

            if (taskInfo == null) throw new BusinessException("阳光体育任务不存在");

            var sunrun = new Sunrun.Sunrun();

            var token = await sunrun.GetToken(taskInfo.ImeiCode);

            await sunrun.StartRun(token, new Models.Services.Sunrun.SunrunUserInfo {
                Latitude = taskInfo.Latitude,
                Length = taskInfo.Length,
                Longitude = taskInfo.Longitude,
                MaxSpeed=taskInfo.MaxSpeed,
                MinSpeed=taskInfo.MinSpeed,
                Name=taskInfo.Name,
                SchoolName=taskInfo.SchoolName,
                Step=taskInfo.Step,
                UserId=taskInfo.UserId
            });

            taskInfo.FailReason = null;

            db.SunrunTasks.Update(taskInfo);

            await db.SaveChangesAsync();

            await using var services = DI.Services.CreateAsyncScope();

            var mailSender = services.ServiceProvider.GetRequiredService<MailSender.MailSender>();

            var userEmail = await db.Users.Where(x => x.Id == taskInfo.BelongTo).AsNoTracking().Select(x=>x.Email).SingleOrDefaultAsync();

            if (userEmail == null) return;

            mailSender.SendRunResultAsync(userEmail, true, null, taskInfo.Name);
        }
    }

    public class SunrunScheduleLogger : JobFilterAttribute, IElectStateFilter
    {
        public void OnStateElection(ElectStateContext context)
        {
            var taskId = context.BackgroundJob.Job.Args.FirstOrDefault() as long?;
            if (taskId == null) return;
            
            switch (context.CandidateState)
            {
                case FailedState failState:
                    ProcessFailState(failState.Exception,context.BackgroundJob.Job.Args).GetAwaiter().GetResult();
                    break;
                case SucceededState:
                        ProcessSuccessState(taskId.Value).GetAwaiter().GetResult();
                    break;
            }
        }

        public async Task ProcessSuccessState(long taskId)
        {
            await using var db = new Database.Database();

            var taskInfo = await (
                    from tasks in db.SunrunTasks
                    join users in db.Users on tasks.BelongTo equals users.Id
                    where tasks.Id == taskId
                    select new
                    {
                        SunrunTaskId=tasks.Id,
                        UserId=users.Id
                    }
                ).AsNoTracking().SingleOrDefaultAsync();

            if (taskInfo == null) return;

            var taskLog = new SunrunLog(taskInfo.SunrunTaskId,taskInfo.UserId,true);

            await db.SunrunLogs.AddAsync(taskLog);

            await db.SaveChangesAsync();
        }

        public async Task ProcessFailState(Exception exception,IReadOnlyList<object> args)
        {
            var (taskId, retryTimes) = (args[0] as long?, args[1] as int?);
            if (taskId==null || retryTimes == null)
            {
                return;
            }
            if(retryTimes <= 3)
            {
                BackgroundJob.Enqueue(() => SunrunSchedule.RunAsync(taskId.Value, retryTimes.Value + 1));
                return;
            }

            await using var db = new Database.Database();

            var taskInfo = await (
                    from tasks in db.SunrunTasks
                    join users in db.Users on tasks.BelongTo equals users.Id
                    where tasks.Id == taskId
                    select new
                    {
                        task = tasks,
                        UserId = users.Id,
                        Email=users.Email
                    }
                ).AsNoTracking().SingleOrDefaultAsync();

            if (taskInfo == null) return;

            var errorMessage = exception switch
            {
                BusinessException ex => ex.Message,
                FlurlHttpTimeoutException => "网络超时",
                FlurlHttpException => "网络访问错误",
                _ => "未知的系统错误"
            };

            var taskLog = new SunrunLog(taskInfo.task.Id, taskInfo.UserId, false,errorMessage);

            await db.SunrunLogs.AddAsync(taskLog);


            var mailSender = DI.Services.GetRequiredService<MailSender.MailSender>();

            mailSender.SendRunResultAsync(taskInfo.Email, false, errorMessage, taskInfo.task.Name);

            var task = taskInfo.task;

            task.FailReason = errorMessage;

            db.SunrunTasks.Update(task);

            await db.SaveChangesAsync();


        }
    }
}

