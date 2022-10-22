using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FuckSunrun.Models.Entities
{
    [Table("sunrun_user")]
    public class SunrunTask
    {
        public SunrunTask()
        {
            IsEnable = true;

            FailReason = null;

            CreateAt = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("imei_code")]
        public string ImeiCode { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("school_name")]
        public string SchoolName { get; set; }

        [Column("min_speed")]
        public double MinSpeed { set; get; }

        [Column("max_speed")]
        public double MaxSpeed { get; set; }

        [Column("length")]
        public int Length { get; set; }

        [Column("latitude")]
        public string Latitude { get; set; }

        [Column("longitude")]
        public string Longitude { get; set; }

        [Column("step")]
        public long Step { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("is_enable")]
        public bool IsEnable { get; set; }

        [Column("fail_reason")]
        public string? FailReason { get; set; }

        [Column("belong_to")]
        public long BelongTo { get; set; }

        [Column("hour")]
        [Range(0,24)]
        public int Hour { get; set; }

        [Column("minute")]
        [Range(0, 60)]
        public int Minute { get; set; }

        [Column("create_at")]
        public long CreateAt { get; set; }
    }
}

