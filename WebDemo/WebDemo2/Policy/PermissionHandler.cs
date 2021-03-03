using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PermissionHandler(IServiceScopeFactory scopeFactory,
            ILogger<PermissionHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            this._logger = logger;
            this._serviceScopeFactory = scopeFactory;
            this._httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            try
            {
                //if (!(context.Resource is AuthorizationFilterContext filterContext))
                //{
                //    //无权限跳转到拒绝页面
                //    context.Fail();
                //    return Task.CompletedTask;
                //}

                //var authorizationFilterContext = context.Resource as AuthorizationFilterContext;

                //var httpContext = authorizationFilterContext?.HttpContext;

                HttpContext httpContext = this._httpContextAccessor.HttpContext;

                var token = TokenRetrieval.FromAuthorizationHeader()(httpContext?.Request);
                if (string.IsNullOrWhiteSpace(token) || token == "null")
                {
                    //无权限跳转到拒绝页面
                    context.Fail();
                    return Task.CompletedTask;
                }

                var user = AuthenticationHelper.GetUserFromToken(token);
                // 是否已经在别的地方登录，或者已经被修改账号密码 
                if (PermissionCache.HavLogin(user.UserName, user.Gid))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                // Get IMemoryCache;
                using IServiceScope scope = _serviceScopeFactory.CreateScope();
                var cache = scope.ServiceProvider.GetService<IMemoryCache>();

                //请求Url
                var questUrl = httpContext?.Request.Path.ToString();

                if (!CheckUserIsExistByMemory(cache, user.UserName, token, user.Gid) || !CheckRequestAuth(user, questUrl))
                {
                    //无权限跳转到拒绝页面
                    context.Fail();
                    return Task.CompletedTask;
                }

                context.Succeed(requirement);
            }
            catch (Exception ex)
            {
                //无权限跳转到拒绝页面
                context.Fail();
                this._logger.LogError("HandleRequirementAsync【验证AccessToken】ex:" + ex.ToString());
            }

            return Task.CompletedTask;
        }

        private bool CheckRequestAuth(UserLogInfo user, string requestUrl)
        {
            if (user == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(user.RoleName))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(requestUrl) || user.IsAdmin)
            {
                return true;
            }

            this._logger.LogDebug("requestUrl ---- " + requestUrl + "| roleName ---- " + user.RoleName);

            try
            {
                return CheckRequestAuthNode(user, requestUrl);
            }
            catch (Exception ex)
            {
                this._logger.LogError("CheckRequestAuth【判断各模块是否有权访问】:" + ex.ToString());
            }

            return false;
        }

        private bool CheckRequestAuthNode(UserLogInfo user, string requestUrl)
        {
            // 这里加入可以允许匿名访问 [AllowAnonymous] 的 Controller
            if (requestUrl.Contains("/Auth/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (var module in PageModules)
            {
                if (!requestUrl.Contains(module.Key, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (requestUrl.Contains(module.Key, StringComparison.OrdinalIgnoreCase) &&
                    module.Key.Equals(module.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // TODO：获取角色相应的访问权限，根据权限判断能否访问模块
                //var systemModules = _permissionCacheService.GetPermissionObj(user.RoleName)?.SystemModule ?? new List<int>();
                //return _equipBaseImpl.CheckGwAddinModules(systemModules, module.Value);
                return true;
            }

            return false;
        }

        #region CheckUserHaveLogIn

        /// <summary>
        /// 检查改用户是否已登陆
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="userName"></param>
        /// <param name="token"></param>
        /// <param name="guid">登录时，PermissionCache会生成一个唯一标识符。现用guid做唯一标识，实际项目中会用token</param>
        /// <returns></returns>
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

            var jwtToken = AuthenticationHelper.GetJwtTokenFromToken(token);
            if (!isManyLoginEnabled)
            {
                // 不支持用户同时登录
                if (memoryGid == guid && jwtToken != default)
                {
                    DateTime nowTime = DateTime.UtcNow;
                    DateTime errorLastTime = jwtToken.ValidTo; // 这里jwt存储的时间是零时区的。
                    TimeSpan ts = nowTime - errorLastTime;
                    if (ts.TotalMinutes <= 120)
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (jwtToken != default)
                {
                    DateTime nowTime = DateTime.UtcNow;
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

        #endregion

        /// <summary>
        /// 用于固化已知的模块
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> PageModules = new ConcurrentDictionary<string, string>()
        {
            // 按照AddinModule表中的ClassName字段进行映射匹配
            ["/UserManage/"] = "UserManage.UserManagePage", // 用户权限

            // 这里的Key和Value相同代表是Controller，不受addinModels表控制
            ["/RabbitDemo/"] = "/RabbitDemo/", // 实时快照
            ["/WeatherForecast/"] = "/WeatherForecast/", // 天气播报
        };
    }
}
