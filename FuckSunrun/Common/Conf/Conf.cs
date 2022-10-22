using System;
namespace FuckSunrun.Common.Conf
{
    public static class Conf
    {
        public static ConfigurationManager Root { get; internal set; }
    }

    public static class ConfAttach
    {
        public static WebApplicationBuilder AttachConfig(this WebApplicationBuilder builder)
        {
            Conf.Root = builder.Configuration;
            return builder;
        }
    }
}

