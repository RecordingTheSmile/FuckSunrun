using System;
using System.Text.RegularExpressions;
using FuckSunrun.Exceptions;
using FuckSunrun.Models.Entities;
using FuckSunrun.Models.Enums;
using FuckSunrun.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace FuckSunrun.Services.Managers
{
    public class UserManager
    {
        /// <summary>
        /// 判断用户是否可以登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="ip">用户登录IP，可空</param>
        /// <returns>用户ID</returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<long> CanLoginAsync(string username,string password,string? ip)
        {
            await using var db = new Database.Database();

            var userInfo = await db.Users.Where(x => x.Username == username).SingleOrDefaultAsync();

            if(userInfo == null)
            {
                throw new BusinessException("用户不存在",404);
            }

            if (!BCrypt.Net.BCrypt.Verify(password, userInfo.Password))
            {
                throw new BusinessException("用户名或密码不正确", 401);
            }

            if (!userInfo.CanLogin)
            {
                throw new BusinessException("您的账户已被封禁", 403);
            }
            if (!userInfo.IsEmailVerified)
            {
                throw new BusinessException("请您先进入邮箱查看确认邮件", 403);
            }

            userInfo.LastLoginAt = DateTimeOffset.Now.ToUnixTimeSeconds();
            userInfo.LastLoginIp = ip;

            db.Users.Update(userInfo);

            await db.SaveChangesAsync();

            return userInfo.Id;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="permission">用户权限，默认为普通用户</param>
        /// <returns>用户ID</returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<long> AddUserAsync(string username,string password,string email,UserPermission permission=UserPermission.User)
        {
            await using var db = new Database.Database();

            if (username.Trim().Length < 4)
            {
                throw new BusinessException("用户名长度必须大于等于4", 400);
            }else if (password.Trim().Length < 5)
            {
                throw new BusinessException("密码长度必须大于等于5", 400);
            }

            if(await db.Users.Where(x=>x.Username == username).AnyAsync())
            {
                throw new BusinessException("该用户名已被使用", 400);
            }

            var userInfo = new User(username, BCrypt.Net.BCrypt.HashPassword(password),email,permission)
                {
                    IsEmailVerified = true
                };

            await db.Users.AddAsync(userInfo);

            await db.SaveChangesAsync();

            return userInfo.Id;
        }

        /// <summary>
        /// 修改用户是否可以登录状态
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="canLogin">是否可以登录</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task ChangeUserCanLoginAsync(long id,bool canLogin)
        {
            await using var db = new Database.Database();

            var userInfo = await db.Users.Where(x => x.Id == id).SingleOrDefaultAsync();

            if (userInfo == null) throw new BusinessException("用户不存在", 404);

            userInfo.CanLogin = canLogin;

            db.Users.Update(userInfo);

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// 修改用户用户名
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="newUsername">新用户名</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task ChangeUserUsernameAsync(long id,string newUsername)
        {
            await using var db = new Database.Database();

            var userInfo = await db.Users.Where(x => x.Id == id).SingleOrDefaultAsync();

            if (userInfo == null) throw new BusinessException("用户不存在", 404);

            if (newUsername.Trim().Length < 4) throw new BusinessException("用户名长度必须大于等于4", 400);

            if (await db.Users.Where(x => x.Username == newUsername && x.Id != id).AnyAsync()) throw new BusinessException("用户名已被使用", 400);

            userInfo.Username = newUsername;

            db.Users.Update(userInfo);

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="newPassword">用户新密码</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task ChangeUserPasswordAsync(long id,string newPassword)
        {
            await using var db = new Database.Database();

            var userInfo = await db.Users.Where(x => x.Id == id).SingleOrDefaultAsync();

            if (userInfo == null) throw new BusinessException("用户不存在", 404);

            if (newPassword.Trim().Length < 5) throw new BusinessException("密码长度必须大于等于5", 400);

            userInfo.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            db.Users.Update(userInfo);

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// 更新用户邮箱
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="email">用户的新邮箱</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task ChangeUserEmailAsync(long id,string email)
        {
            await using var db = new Database.Database();

            var userInfo = await db.Users.Where(x => x.Id == id).SingleOrDefaultAsync();

            if (userInfo == null) throw new BusinessException("用户不存在", 404);

            if (!Regex.IsMatch(email, @"(.+)@(.+)\.(.+)")) throw new BusinessException("邮箱格式不正确", 400);

            if (await db.Users.Where(x => x.Email == email && x.Id != id).AnyAsync()) throw new BusinessException("邮箱已被使用", 400);

            userInfo.Email = email;

            db.Users.Update(userInfo);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// 修改用户邮箱验证状态
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="isEmailVerified">邮箱是否验证</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task ChangeUserEmailStatusAsync(long id,bool isEmailVerified)
        {
            await using var db = new Database.Database();

            var userInfo = await db.Users.Where(x => x.Id == id).SingleOrDefaultAsync();

            if (userInfo == null) throw new BusinessException("用户不存在", 404);

            userInfo.IsEmailVerified = isEmailVerified;

            db.Users.Update(userInfo);

            await db.SaveChangesAsync();
        }
    }

    public static class InjectUserManager
    {
        public static IServiceCollection AddUserManager(this IServiceCollection services)
        {
            services.AddSingleton<UserManager>();

            return services;
        }
    }
}

