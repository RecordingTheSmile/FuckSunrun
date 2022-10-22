using System;
using Hangfire;

namespace FuckSunrun.Services.Schedule
{
    public class HangfireBackgroundJob:BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var server = new Hangfire.BackgroundJobServer(new BackgroundJobServerOptions
            {
                WorkerCount = 20
            });

            return server.WaitForShutdownAsync(stoppingToken);
        }
    }
}

