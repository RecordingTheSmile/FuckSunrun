using System;
using FuckSunrun.Exceptions;
using FuckSunrun.Models.Entities;
using FuckSunrun.Models.Services.Sunrun;
using FuckSunrun.Services.Schedule;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace FuckSunrun.Services.Managers
{
    public class SunrunTaskManage
    {
        private readonly static TimeZoneInfo TZAsiaShanghai = TimeZoneConverter.TZConvert.GetTimeZoneInfo("Asia/Shanghai");

        public async Task AddTaskAsync(long userId,string imeiCode,bool isEnable,int hour,int minute,SunrunUserInfo userinfo)
        {
            await using var db = new Database.Database();

            var task = new SunrunTask {
                ImeiCode=imeiCode,
                BelongTo=userId,
                SchoolName=userinfo.SchoolName,
                CreateAt=DateTimeOffset.Now.ToUnixTimeSeconds(),
                FailReason=null,
                Step=userinfo.Step,
                MaxSpeed=userinfo.MaxSpeed,
                MinSpeed=userinfo.MinSpeed,
                IsEnable=isEnable,
                Latitude=userinfo.Latitude,
                Longitude=userinfo.Longitude,
                Length=userinfo.Length,
                Name=userinfo.Name,
                UserId=userinfo.UserId,
                Hour=hour,
                Minute=minute
            };

            await db.SunrunTasks.AddAsync(task);

            await db.SaveChangesAsync();

            if (isEnable)
            {
                RecurringJob.AddOrUpdate($"{task.Id}:{task.UserId}:{task.CreateAt}", () => SunrunSchedule.RunAsync(task.Id,0), Cron.Daily(hour, minute), TZAsiaShanghai);
            }
        }

        public async Task DeleteTaskAsync(long taskId)
        {
            await using var db = new Database.Database();
            var task = await db.SunrunTasks.Where(x => x.Id == taskId)
                .SingleOrDefaultAsync();

            if (task == null) throw new BusinessException("任务不存在", 404);

            RecurringJob.RemoveIfExists($"{task.Id}:{task.UserId}:{task.CreateAt}");

            db.SunrunTasks.Remove(task);

            await db.SaveChangesAsync();
        }

        public async Task ChangeTaskTimeAsync(long taskId,int hour,int minute)
        {
            await using var db = new Database.Database();
            var task = await db.SunrunTasks.Where(x => x.Id == taskId)
                .SingleOrDefaultAsync();

            if (task == null) throw new BusinessException("任务不存在", 404);
            RecurringJob.RemoveIfExists($"{task.Id}:{task.UserId}:{task.CreateAt}");

            if (task.IsEnable)
            {
                RecurringJob.AddOrUpdate($"{task.Id}:{task.UserId}:{task.CreateAt}", () => SunrunSchedule.RunAsync(task.Id,0), Cron.Daily(hour, minute), TZAsiaShanghai);
            }

            task.Hour = hour;
            task.Minute = minute;

            db.SunrunTasks.Update(task);

            await db.SaveChangesAsync();
        }

        public async Task ChangeTaskStatusAsync(long taskId,bool isEnable)
        {
            await using var db = new Database.Database();
            var task = await db.SunrunTasks.Where(x => x.Id == taskId)
                .SingleOrDefaultAsync();

            if (task == null) throw new BusinessException("任务不存在", 404);

            task.IsEnable = isEnable;

            RecurringJob.RemoveIfExists($"{task.Id}:{task.UserId}:{task.CreateAt}");

            if (isEnable)
            {
                RecurringJob.AddOrUpdate($"{task.Id}:{task.UserId}:{task.CreateAt}", () => SunrunSchedule.RunAsync(task.Id,0), Cron.Daily(task.Hour, task.Minute), TZAsiaShanghai);
            }

            db.SunrunTasks.Update(task);

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// 立即执行任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <exception cref="BusinessException"></exception>
        public async Task ExecuteNowAsync(long taskId)
        {
            await using var db = new Database.Database();
            var task = await db.SunrunTasks.Where(x => x.Id == taskId)
                .SingleOrDefaultAsync();

            if (task == null) throw new BusinessException("任务不存在", 404);

            RecurringJob.TriggerJob($"{task.Id}:{task.UserId}:{task.CreateAt}");
        }
    }

    public static class InjectSunrunTaskManage
    {
        public static IServiceCollection AddSunrunTaskManager(this IServiceCollection services)
        {
            services.AddSingleton<SunrunTaskManage>();

            return services;
        }
    }
}

