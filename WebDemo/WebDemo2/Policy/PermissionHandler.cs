using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebDemo2.Models;
using WebDemo2.Service;

namespace WebDemo2.Policy
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionHandler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public PermissionHandler(IServiceScopeFactory scopeFactory,
            ILogger<PermissionHandler> logger)
        {
            this._logger = logger;
            this._serviceScopeFactory = scopeFactory;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            try
            {
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                var cache = scope.ServiceProvider.GetService<IMemoryCache>();

                if (!(context.Resource is AuthorizationFilterContext filterContext))
                {
                    //无权限跳转到拒绝页面
                    context.Fail();
                }

                var authorizationFilterContext = context.Resource as AuthorizationFilterContext;

                var httpContext = authorizationFilterContext?.HttpContext;

                //请求Url
                var questUrl = httpContext?.Request.Path.ToString();

                var token = TokenRetrieval.FromAuthorizationHeader()(httpContext?.Request);
                if (string.IsNullOrWhiteSpace(token) || token == "null")
                {
                    //无权限跳转到拒绝页面
                    context.Fail();
                    return;
                }

                var user = AuthenticationHelper.GetUserFromToken(token);
                // 是否已经在别的地方登录，或者已经被修改账号密码 
                if (PermissionCache.HavLogin(user.UserName, user.Gid))
                {
                    context.Fail();
                    return;
                }

                if (!CheckUserIsExistByMemory(cache, user.UserName, token, user.Gid) || !CheckRequestAuth(user))
                {
                    //无权限跳转到拒绝页面
                    context.Fail();
                }

                context.Succeed(requirement);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {

                //无权限跳转到拒绝页面
                context.Fail();
                this._logger.LogError("HandleRequirementAsync【验证AccessToken】ex:" + ex.ToString());
            }
        }

        private bool CheckRequestAuth(UserLogInfo user)
        {
            return true;
        }

        private bool CheckUserIsExistByMemory(IMemoryCache cache, string userName, string token, string guid)
        {
            bool flag = false;
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                    return flag;

                var userMemoryInfo = cache.Get<UserLogInfo>(userName);
                if (userMemoryInfo != null)
                {
                    flag = CheckManyLogin(userMemoryInfo, token, guid);
                }

                if (!flag)
                {
                    this._logger.LogError("CheckUserIsExist【校验用户是否已登录】admin:" + userName + "未登录平台");
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError("CheckUserIsExist【验证AccessToken】:" + ex.ToString());
            }

            return flag;
        }

        private bool CheckManyLogin(UserLogInfo userMemoryInfo, string token, string guid)
        {
            var memoryGid = userMemoryInfo.Gid;
            if (string.IsNullOrEmpty(memoryGid))
                return false;

            var isManyLoginEnabled =
                Convert.ToBoolean(Startup.GetConfiguration().GetSection("AllowedMultiLogin").Value);
            if (!isManyLoginEnabled)
            {
                // 不支持用户同时登录
                var jwtToken = AuthenticationHelper.GetJwtTokenFromToken(token);
                if (memoryGid == guid && jwtToken != default)
                {
                    DateTime nowTime = DateTime.Now;
                    DateTime errorLastTime = jwtToken.ValidTo;
                    TimeSpan ts = nowTime - errorLastTime;
                    if (ts.TotalMinutes <= 120)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
