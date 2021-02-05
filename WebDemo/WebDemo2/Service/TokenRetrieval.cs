using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebDemo2.Service
{
    public class TokenRetrieval
    {
        /// <summary>
        /// 读取HttpRequestHeader携带的认证Token。并截取真正的JWT
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        public static Func<HttpRequest, string> FromAuthorizationHeader(string scheme = "Bearer")
        {
            return (request) =>
            {
                string authorization = request.Headers[HttpRequestHeader.Authorization.ToString()].FirstOrDefault();

                if (string.IsNullOrEmpty(authorization))
                {
                    return null;
                }

                if (authorization.StartsWith(scheme + " ", StringComparison.OrdinalIgnoreCase))
                {
                    return authorization.Substring(scheme.Length + 1).Trim();
                }

                return null;
            };
        }

        public static Func<HttpRequest, string> FromQueryString(string name = "access_token")
        {
            return (request) => request.Query[name].FirstOrDefault();
        }
    }
}
