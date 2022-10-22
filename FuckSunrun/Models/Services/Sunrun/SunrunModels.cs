using System;
namespace FuckSunrun.Models.Services.Sunrun
{
    public class SunrunRunResult
    {
        public string CostTime { get; set; }
        public int CostDistance { get; set; }
        public int AvaLengths { get; set; }
        public string ResultDate { get; set; }
        public int ResultHour { get; set; }
        public int StepNum { get; set; }
        public string NoCountReason { get; set; }
        public double Speed { get; set; }
    }

    public class SunrunRunTimes
    {
        public int RaceNums { get; set; } //普通跑步次数
        public int RaceMNums { get; set; } //晨跑次数
        public int TotalTimes => RaceNums + RaceMNums; //总次数
    }

    public class SunrunUserInfo
    {
        public string Name { get; set; }
        public string SchoolName { get; set; }
        public double MinSpeed { set; get; }
        public double MaxSpeed { get; set; }
        public int Length { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public long Step { get; set; }
        public string UserId { get; set; }
    }
}

