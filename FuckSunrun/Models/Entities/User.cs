using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FuckSunrun.Models.Enums;

namespace FuckSunrun.Models.Entities
{
    

    [Table("user")]
    public class User
    {
        public User(string username,string password,string email,UserPermission permission=UserPermission.User)
        {
            CanLogin = true;
            CreateAt = DateTimeOffset.Now.ToUnixTimeSeconds();
            LastLoginAt = DateTimeOffset.Now.ToUnixTimeSeconds();
            Username = username;
            Password = password;
            Email = email;
            Permission = permission;
            IsEmailVerified = false;
        }

        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity),Column("id")]
        public long Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("email")]
        [RegularExpression(@"(.+)@(.+)\.(.+)",ErrorMessage ="请输入正确的邮箱")]
        public string Email { get; set; }

        [Column("can_login")]
        public bool CanLogin { get; set; }

        [Column("create_at")]
        public long CreateAt { get; set; }

        [Column("last_login_at")]
        public long LastLoginAt { get; set; }

        [Column("permission")]
        public UserPermission Permission { get; set; }

        [Column("last_login_ip")]
        public string? LastLoginIp { get; set; }

        [Column("is_email_verified")]
        public bool IsEmailVerified { get; set; }
    }
}

