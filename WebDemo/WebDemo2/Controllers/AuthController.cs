using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebDemo2.Models;
using WebDemo2.Service;

namespace WebDemo2.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMemoryCache _Cache;
        private readonly CaptchaHelper _CaptchaHelper;
        private readonly string IMAGEPATH = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "gallery");
        public AuthController(CaptchaHelper captchaHelper, IMemoryCache cache)
        {
            this._Cache = cache;
            this._CaptchaHelper = captchaHelper;
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet("Verification")]
        public JsonResult GetVerification()
        {
            var result = this._CaptchaHelper.GetVerificationCode(this.IMAGEPATH);
            this._Cache.Set("code", this._CaptchaHelper._PositionX, DateTimeOffset.Now.AddMinutes(1));
            this._Cache.Set("code_errornum", string.Empty);
            return new JsonResult(ResponseModel.GetResultSuccess(result));
        }


        /// <summary>
        /// 检查滑动验证码
        /// </summary>
        /// <returns></returns>
        [HttpPost("Check")]
        public IActionResult Check([FromForm] int point, [FromForm] int timespan, [FromForm] string datelist)
        {
            var date_list = datelist;
            if (string.IsNullOrEmpty(date_list))
            {
                return Ok(new { state = -1, msg = "参数date_list为空" });
            }

            if (point <= 0)
            {
                return Ok(new { state = -1, msg = "坐标值为空" });
            }

            var code = Convert.ToString(_Cache.Get("code"));
            if (string.IsNullOrEmpty(code) || !int.TryParse(code, out var old_point))
            {
                return Ok(new { state = -2, msg = "验证码已过期请刷新重试" });
            }

            //错误
            if (Math.Abs(old_point - point) > CaptchaHelper._deviationPx)
            {
                var li_count = 0;
                var errorNum = Convert.ToString(this._Cache.Get("code_errornum"));
                if (!string.IsNullOrEmpty(errorNum) && int.TryParse(errorNum, out var num))
                {
                    li_count = num;
                }
                li_count++;

                if (li_count > CaptchaHelper._MaxErrorNum)
                {
                    //超过最大错误次数后不再校验
                    this._Cache.Set("code", 0);

                    return Ok(new { state = -1, msg = "超过最大错误次数" });

                }

                this._Cache.Set("code_errornum", li_count.ToString());

                //返回错误次数
                return Ok(new { state = -1, msg = $"第【{li_count}】次验证错误" });
            }


            //校验成功 返回正确坐标
            this._Cache.Set("isCheck", "OK");
            this._Cache.Set("code_errornum", string.Empty);
            this._Cache.Set("code", string.Empty);

            return Ok(new { state = 0, info = "正确", data = point });
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserLogInfo logInfo)
        {
            if (!string.IsNullOrEmpty(logInfo.UserName) && !string.IsNullOrEmpty(logInfo.Password))
            {                
                var tokenString = AuthenticationHelper.GenerateToken(logInfo);

                this._Cache.Set(logInfo.UserName, logInfo); // 将用户登录信息加入缓存。用于后续的权限管理等

                return new JsonResult(new ResponseModel
                {
                    Code = 0,
                    Message = "登录成功，欢迎 " + logInfo.UserName,
                    Data = tokenString
                });
            }
            else
            {
                return ResponseModel.JsonResultResponseFaild("username or password is incorrect.");
            }
        }
    }
}
