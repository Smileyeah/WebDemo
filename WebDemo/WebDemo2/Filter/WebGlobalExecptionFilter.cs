using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebDemo2.Models;

namespace WebDemo2.Filter
{
    public class WebGlobalExecptionFilter : IExceptionFilter
    {
        private readonly ILogger<WebGlobalExecptionFilter> _log;

        public WebGlobalExecptionFilter(ILogger<WebGlobalExecptionFilter> log)
        {
            _log = log;
        }

        public void OnException(ExceptionContext context)
        {
            _log.LogError($"错误异常：{context.Exception}");
            var result = new ResponseModel()
            {
                Code = 0x0001,//系统异常代码
                Message = "系统内部异常",//系统异常信息
            };
            context.Result = new ObjectResult(result);
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.ExceptionHandled = true;
        }
    }
}
