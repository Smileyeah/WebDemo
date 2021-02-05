using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDemo2.Policy
{
    /// <summary>
    /// 权限承载实体
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// 无权限action
        /// </summary>
        public string DeniedAction { get; set; }

        /// <summary>
        /// 认证授权类型
        /// </summary>
        public string ClaimType { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public TimeSpan Expiration { get; set; }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="deniedAction"></param>
        /// <param name="claimType"></param>
        /// <param name="expiration"></param>
        public PermissionRequirement(string claimType, TimeSpan expiration, string deniedAction = "/api/nopermission")
        {
            ClaimType = claimType;
            Expiration = expiration;
            DeniedAction = deniedAction;
        }
    }
}
