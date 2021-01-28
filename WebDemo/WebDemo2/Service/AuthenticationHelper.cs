using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace WebDemo2.Service
{
    public class AuthenticationHelper
    {
        public static string GenerateToken(string userName)
        {
            IConfiguration configuration = Startup.GetConfiguration();

            var claims = new[]
                   {
                    new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") ,
                    new Claim(JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddMinutes(30)).ToUnixTimeSeconds()}"),
                    new Claim(ClaimTypes.Name, userName),
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
    }
}
