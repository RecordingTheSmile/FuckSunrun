using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FuckSunrun.Models.Dtos
{
    public class PostLoginBody
    {
        [Required(AllowEmptyStrings =false,ErrorMessage ="用户名不得为空")]
        [MinLength(4,ErrorMessage ="用户名不得少于4位")]
        public string Username { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "密码不得为空")]
        [MinLength(5,ErrorMessage = "密码不得少于5位")]
        public string Password { get; set; }

        public string? Remember { get; set; }
    }

    public class PostRegisterBody
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "用户名不得为空")]
        [MinLength(4, ErrorMessage = "用户名不得少于4位")]
        public string Username { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "密码不得为空")]
        [MinLength(5, ErrorMessage = "密码不得少于5位")]
        public string Password { get; set; }

        [Required(AllowEmptyStrings =false,ErrorMessage ="邮箱不得为空")]
        [RegularExpression(@"(.+)@(.+)\.(.+)",ErrorMessage ="请输入正确的邮箱")]
        public string Email { get; set; }
        
        [Required(AllowEmptyStrings =false,ErrorMessage ="验证码不得为空")]
        [MaxLength(20,ErrorMessage ="验证码不得多于20位")]
        public string Code { get; set; }
    }

    public class PostSendEmailCodeBody
    {
        [Required(AllowEmptyStrings =false,ErrorMessage ="验证码不得为空")]
        [MaxLength(20,ErrorMessage ="验证码不得多于20位")]
        public string Code { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "邮箱不得为空")]
        [RegularExpression(@"(.+)@(.+)\.(.+)", ErrorMessage = "请输入正确的邮箱")]
        public string Email { get; set; }
    }

    public class PutResetPasswordBody
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "密码不得为空")]
        [MinLength(5, ErrorMessage = "密码不得少于5位")]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "验证码不得为空")]
        [MaxLength(20, ErrorMessage = "验证码不得多于20位")]
        public string Code { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "邮箱不得为空")]
        [RegularExpression(@"(.+)@(.+)\.(.+)", ErrorMessage = "请输入正确的邮箱")]
        public string Email { get; set; }
    }

    public class GetTasksQuery: BasePaginationModel
    {

    }

    public class PostTaskBody
    {
        [Required(AllowEmptyStrings =false,ErrorMessage ="IMEICODE不得为空")]
        public string ImeiCode { get; set; }

        [BindRequired]
        [Range(0,24)]
        public int Hour { get; set; }

        [BindRequired]
        [Range(0, 60)]
        public int Minute { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Lat不得为空")]
        public string Latitude { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Lon不得为空")]
        public string Longitude { get; set; }

        [BindRequired]
        [Range(0,int.MaxValue)]
        public int Step { get; set; }

        [BindRequired]
        public bool IsEnable { get; set; }
    }

    public class PutTaskBody
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "IMEICODE不得为空")]
        public string ImeiCode { get; set; }

        [BindRequired]
        [Range(0, 24)]
        public int Hour { get; set; }

        [BindRequired]
        [Range(0, 60)]
        public int Minute { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Lat不得为空")]
        public string Latitude { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Lon不得为空")]
        public string Longitude { get; set; }

        [BindRequired]
        [Range(0, int.MaxValue)]
        public int Step { get; set; }

        [BindRequired]
        public bool IsEnable { get; set; }
    }

    public class GetTaskLogsQuery : BasePaginationModel { }

    public class GetUsersQuery : BasePaginationModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
    }

    public class PutUserLoginStatusBody
    {
        [BindRequired]
        public bool CanLogin { get; set; }
    }

    public class PutTaskEnableBody
    {
        [BindRequired]
        public bool IsEnable { get; set; }
    }

    public class PutUserEmailStatusBody
    {
        [BindRequired]
        public bool IsEmailVerified { get; set; }
    }

    public class PutMeUsernameBody
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "用户名不得为空")]
        [MinLength(4, ErrorMessage = "用户名不得少于4位")]
        public string Username { get; set; }
    }

    public class PutMePasswordBody
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "密码不得为空")]
        [MinLength(5, ErrorMessage = "密码不得少于5位")]
        public string Password { get; set; }
    }

    public class PutMeEmailBody
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "邮箱不得为空")]
        [RegularExpression(@"(.+)@(.+)\.(.+)", ErrorMessage = "请输入正确的邮箱")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "验证码不得为空")]
        [MaxLength(20, ErrorMessage = "验证码不得多于20位")]
        public string Code { get; set; }
    }
}

