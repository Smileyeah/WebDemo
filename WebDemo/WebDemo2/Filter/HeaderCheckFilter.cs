using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebDemo2.Service;

namespace WebDemo2.Filter
{
    /// <summary>
    /// check
    /// </summary>
    public class HeaderCheckFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {

            bool hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
                                .Any(em => em.GetType() == typeof(AllowAnonymousAttribute)); //< -- Here it is

            if (hasAllowAnonymous) return;

            base.OnActionExecuting(context);

            var token = TokenRetrieval.FromAuthorizationHeader()(context?.HttpContext.Request);
            if (string.IsNullOrWhiteSpace(token) || token == "null")
            {
                //无权限跳转到拒绝页面

                context.Result = new JsonResult(new { code = 401, message = "校验不成功!", data = false })
                {
                    StatusCode = 401
                };

                base.OnActionExecuting(context);
            }
        }
    }
}
