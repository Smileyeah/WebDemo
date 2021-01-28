using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserLogInfo logInfo)
        {
            if (!string.IsNullOrEmpty(logInfo.UserName) && !string.IsNullOrEmpty(logInfo.Password))
            {                
                var tokenString = AuthenticationHelper.GenerateToken(logInfo.UserName);

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
