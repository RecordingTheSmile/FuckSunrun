using System;
using FuckSunrun.Common.Conf;
using FuckSunrun.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FuckSunrun.Services.Database
{
    public class Database : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<SunrunTask> SunrunTasks { get; set; }

        public DbSet<SunrunLog> SunrunLogs { get; set; }

        public Database(DbContextOptions<Database> options) : base(options) { }

        public Database() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(optionsBuilder.IsConfigured)
                base.OnConfiguring(optionsBuilder);
            else
            {
                optionsBuilder.UseNpgsql(Conf.Root.GetConnectionString("PgSQL"));
            }
        }
    }
}

