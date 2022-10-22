using System;
using Microsoft.AspNetCore.Rewrite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Web;
using FuckSunrun.Models.Services.Sunrun;
using FuckSunrun.Exceptions;
using Flurl;
using Flurl.Http;

namespace FuckSunrun.Services.Sunrun
{
    public class Sunrun
    {
        private const string BaseUrl = "http://client3.aipao.me/api";

        public async Task<bool> VerifyImeiCode(string imeicode, bool IsIphone = false)
        {
            string token;
            try
            {
                token = await GetToken(imeicode, IsIphone);
            }
            catch (Exception)
            {
                return false;
            }

            return !string.IsNullOrEmpty(token);
        }

        public async Task<string> GetUserName(string imeicode, bool isIphone = false)
        {
            var token = await GetToken(imeicode, isIphone);
            var userinfo = await GetUserInfo(token);
            return userinfo.Name + userinfo.UserId;
        }

        public async Task<IEnumerable<SunrunRunResult>> GetResultOfValid(string imeicode, int page, int limit, bool isIphone = false)
        {
            var token = await GetToken(imeicode, isIphone);
            var userinfo = await GetUserInfo(token);
            var query = new Dictionary<string, string>();
            query.Add("UserId", userinfo.UserId);
            query.Add("pageIndex", page.ToString());
            query.Add("pageSize", limit.ToString());
            var result = await Get("/" + token + "/QM_Runs/getResultsofValidByUser", query);
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (!(result["Success"]?.ToObject<bool>() ?? false)) throw new BusinessException(result["ErrMsg"]?.ToString() ?? "未知错误");

            return result["listValue"]?.ToObject<IEnumerable<SunrunRunResult>>() ?? Array.Empty<SunrunRunResult>();
        }

        public async Task<IEnumerable<SunrunRunResult>> GetResultOfInvalid(string imeicode, int page, int limit,
            bool isIphone = false)
        {
            var token = await GetToken(imeicode, isIphone);
            var userinfo = await GetUserInfo(token);
            var query = new Dictionary<string, string>();
            query.Add("UserId", userinfo.UserId);
            query.Add("pageIndex", page.ToString());
            query.Add("pageSize", limit.ToString());
            var result = await Get("/" + token + "/QM_Runs/getResultsofInValidByUser", query);
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (!(result["Success"]?.ToObject<bool>() ?? false)) throw new BusinessException(result["ErrMsg"]?.ToString() ?? "未知错误");

            return result["listInValue"]?.ToObject<IEnumerable<SunrunRunResult>>() ?? Array.Empty<SunrunRunResult>();
        }

        public async Task<SunrunRunTimes> GetRunTimes(string imeicode, bool isIphone = false)
        {
            var token = await GetToken(imeicode, isIphone);
            var userinfo = await GetUserInfo(token);
            var query = new Dictionary<string, string>();
            query.Add("UserId", userinfo.UserId);
            query.Add("pageIndex", "1");
            query.Add("pageSize", "10000");
            var result = await Get("/" + token + "/QM_Runs/getResultsofValidByUser", query);
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (!(result["Success"]?.ToObject<bool>() ?? false)) throw new BusinessException(result["ErrMsg"]?.ToString() ?? "未知错误");

            var runTimes = new SunrunRunTimes();
            runTimes.RaceNums = result["RaceNums"]?.ToObject<int>()??0;
            runTimes.RaceMNums = result["RaceMNums"]?.ToObject<int>()??0;
            return runTimes;
        }

        public async Task<string> GetToken(string ImeiCode, bool IsIphone = false)
        {
            var query = new Dictionary<string, string>();
            query.Add("IMEICode", ImeiCode);
            var result =
                await Get(
                    IsIphone ? "/%7Btoken%7D/QM_Users/LoginSchool" : "/%7Btoken%7D/QM_Users/Login_AndroidSchool",
                    query);
            if (!(result?.Value<bool>("Success") ?? false)) throw new BusinessException(result?["ErrMsg"]?.ToString() ?? "未知错误");

            return result?["Data"]?["Token"]?.ToString() ?? String.Empty;
        }

        public async Task<SunrunUserInfo> GetUserInfo(string token)
        {
            var result = await Get("/" + token + "/QM_Users/GS", null);
            if (!(result?.Value<bool>("Success") ?? false)) throw new BusinessException(result?["ErrMsg"]?.ToString() ?? "未知错误");

            var user = result?["Data"]?["User"];
            var schoolRun = result?["Data"]?["SchoolRun"];

            if(user == null || schoolRun == null)
            {
                throw new BusinessException("用户信息为空");
            }

            var userinfo = new SunrunUserInfo();
            userinfo.Length = schoolRun.Value<int>("Lengths");
            userinfo.MaxSpeed = schoolRun.Value<double>("MaxSpeed");
            userinfo.MinSpeed = schoolRun.Value<double>("MinSpeed");
            userinfo.SchoolName = schoolRun.Value<string>("SchoolName") ?? String.Empty;
            userinfo.Name = user.Value<string>("NickName") ?? String.Empty;
            userinfo.UserId = user.Value<int>("UserID").ToString();
            return userinfo;
        }

        public async Task StartRun(string token, SunrunUserInfo userInfo)
        {
            var runInfoUri = new Dictionary<string, string>();
            runInfoUri.Add("S3", userInfo.Length.ToString());
            runInfoUri.Add("S1", userInfo.Latitude);
            runInfoUri.Add("S2", userInfo.Longitude);
            var runInfoResult = await Get("/" + token + "/QM_Runs/SRS", runInfoUri);
            if (!runInfoResult.Value<bool>("Success"))
                throw new Exception(runInfoResult?["ErrMsg"]?.ToString() ?? "未知错误");

            var route = runInfoResult?["Data"]?["Routes"]?.ToString();
            var runid = runInfoResult?["Data"]?["RunId"]?.ToString();
            var randomspeed = (userInfo.MaxSpeed + userInfo.MinSpeed) / 2 +
                              new Random().NextDouble() * (userInfo.MaxSpeed - userInfo.MinSpeed - 0.2) -
                              (userInfo.MaxSpeed - userInfo.MinSpeed) / 2;
            var randomruntime = Math.Round(userInfo.Length / randomspeed);
            double randomrunlength = userInfo.Length + new Random().Next(1);
            var runUri = new Dictionary<string, string>();
            var crypto = new Crypto();
            var key = crypto.EncryptKey();
            runUri.Add("S4", crypto.Encrypt(key, Convert.ToInt32(randomruntime)));
            runUri.Add("S5", crypto.Encrypt(key, Convert.ToInt32(randomrunlength)));
            runUri.Add("S6", route ?? String.Empty);
            runUri.Add("S7", "1");
            runUri.Add("S8", key);
            runUri.Add("S9", crypto.Encrypt(key, (int)(userInfo.Step + new Random().Next(101) - 50)));
            runUri.Add("S1", runid ?? String.Empty);
            var result = await Get("/" + token + "/QM_Runs/ES", runUri);
            if (!(result["Success"]?.ToObject<bool>() ?? false)) throw new BusinessException(result["ErrMsg"]?.ToString() ?? "未知错误");
        }

        private async Task<JObject> Get(string uri, Dictionary<string, string>? query)
        {
            var url = BaseUrl + uri;

            if(query != null)
            {
                var queryStringBuilder = new StringBuilder();

                queryStringBuilder.Append('?');
                
                foreach (var item in query)
                {
                    queryStringBuilder.Append(item.Key);
                    queryStringBuilder.Append('=');
                    queryStringBuilder.Append(HttpUtility.UrlEncode(item.Value));
                    queryStringBuilder.Append('&');
                }

                var queryString = queryStringBuilder.ToString().Trim('&').Trim();

                url += queryString;
            }

            var request = url.WithHeader("version","9.9");

            if (url.Contains("/QM_Users/GS"))
            {
                request.WithHeader("auth", new Crypto().UserInfoEncrypt());
            }

            return await request.GetJsonAsync<JObject>();
        }
    }
}

