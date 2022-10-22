using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuckSunrun.Models.Entities
{
    public class SunrunLog
    {
        //public SunrunLog() { }

        public SunrunLog(long sunrunTaskId,long belongTo,bool isSuccess,string? failReason = null)
        {
            SunrunTaskId = sunrunTaskId;
            IsSuccess = isSuccess;
            BelongTo = belongTo;
            FailReason = failReason;
            CreateAt = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long SunrunTaskId { get; set; }

        public bool IsSuccess { get; set; }

        public string? FailReason { get; set; }

        public long CreateAt { get; set; }

        public long BelongTo { get; set; }
    }
}

