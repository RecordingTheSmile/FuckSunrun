using System;
namespace FuckSunrun.Common.DI
{
    public static class DI
    {
        public static IServiceProvider Services { get; internal set; }
    }

    public static class DIAttach
    {
        public static WebApplication AttachDI(this WebApplication app)
        {
            DI.Services = app.Services;
            return app;
        }
    }
}

