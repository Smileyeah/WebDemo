using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebDemo2.Models;

namespace WebDemo2.Service
{
    public class AuthenticationHelper
    {
        public static string GenerateToken(UserLogInfo userLogInfo)
        {
            IConfiguration configuration = Startup.GetConfiguration();

            var claims = new[]
                   {
                    new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") ,
                    new Claim(JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddMinutes(30)).ToUnixTimeSeconds()}"),
                    new Claim(ClaimTypes.Name, userLogInfo.UserName),
                    new Claim(ClaimTypes.Role, userLogInfo.RoleName),
                    new Claim(ClaimTypes.Sid, userLogInfo.Gid = PermissionCache.UserLogin(userLogInfo.UserName)),
                    new Claim(ClaimTypes.DateOfBirth, $"{DateTime.Now.AddYears(-22)}")
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtBearer:SecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: configuration["JwtBearer:Audience"],
                audience: configuration["JwtBearer:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            try
            {
                string tokenstr = new JwtSecurityTokenHandler().WriteToken(token);
                return tokenstr;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return default;
            }
        }

        public static UserLogInfo GetUserFromToken(string jwtToken)
        {
            var userInfo = new UserLogInfo();

            if (string.IsNullOrWhiteSpace(jwtToken))
                return userInfo;

            var result = new JwtSecurityTokenHandler();
            try
            {
                var resultData = result.ReadJwtToken(jwtToken);
                userInfo.UserName = resultData.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Name)?.Value;
                userInfo.RoleName = resultData.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Role)?.Value;
                if (userInfo.RoleName != null)
                {
                    userInfo.IsAdmin = userInfo.RoleName.ToLower(CultureInfo.CurrentCulture)
                        .Equals("admin", StringComparison.OrdinalIgnoreCase);
                }

                userInfo.Gid = resultData.Claims.FirstOrDefault(m => m.Type == ClaimTypes.Sid)?.Value;
            }
            catch (Exception ex)
            {
                var oriColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("HandleRequirementAsync【Token解析异常】,jwt:" + jwtToken + ";ex:" + ex.ToString());
                Console.ForegroundColor = oriColor;
            }

            return userInfo;
        }


        public static JwtSecurityToken GetJwtTokenFromToken(string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
                return default;

            try
            {
                var result = new JwtSecurityTokenHandler();

                return result.ReadJwtToken(jwtToken);
            }
            catch (Exception ex)
            {
                var oriColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("HandleRequirementAsync【Token解析异常】,jwt:" + jwtToken + ";ex:" + ex.ToString());
                Console.ForegroundColor = oriColor;
            }

            return default;
        }
    }
}
