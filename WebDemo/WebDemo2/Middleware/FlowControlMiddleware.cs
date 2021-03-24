using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebDemo2.Extensions;
using WebDemo2.Models;

namespace WebDemo2.Middleware
{
    public class FlowControlMiddleware : IMiddleware
    {
        private long _count;
        private readonly MemoryCache _memoryCache;
        private readonly SentinelCollectionExtensions _sentinelCollection;//记录类
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FlowControlMiddleware> _apiLog;
        private readonly IConfiguration _configuration;

        public FlowControlMiddleware(MemoryCache memoryCache,
            SentinelCollectionExtensions sentinelCollection,
            IServiceProvider serviceProvider,
            ILogger<FlowControlMiddleware> apiLog)
        {
            _memoryCache = memoryCache;
            _sentinelCollection = sentinelCollection;
            //_memoryCache.Set("ApiList", _sentinelCollection.GetAPiList());//读取需要流量控制接口配置文件
            _serviceProvider = serviceProvider;
            _apiLog = apiLog;

            _configuration = Startup.GetConfiguration();
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            //var apiInfo = new ApiFlow();
            //DateTime oneTime = DateTime.Now;//开始访问时间
            //Interlocked.Increment(ref _count);
            //_sentinelCollection.TotalVisit = _count;//设置总访问量
            //apiInfo.ApiName = context.Request.Path.ToString();//记录接口信息
            //apiInfo.CreateTime = oneTime;
            //apiInfo.UpBytes = await GetRequestBytes(context);//记录接口上行数据
            //apiInfo.Ip = context.Connection.RemoteIpAddress.MapToIPv4().ToString();//获取IP
            //_apiLog.LogInformation($"流控策略开始执行，Ip：{apiInfo.Ip}");
            //var ApiList = _memoryCache.Get<List<IoTNorthApiList>>("ApiList");
            //var ApiUrlList = ApiList.Select(a => a.RequestUrl).ToList();//流量控制接口列表
            //var apiId = ApiList.Where(a => a.RequestUrl.Equals(context.Request.Path.ToString(), StringComparison.CurrentCultureIgnoreCase))
            //    .Select(a => a.Id).FirstOrDefault();
            //if (apiId != null)
            //{
            //    var appAuth = _sentinelCollection.GetAppId(context, apiId);//通过请求token获取用户信息。
            //    if (appAuth == null)
            //    {
            //        _apiLog.LogInformation($"流控策略执行中，找不到{apiId}的授权信息");
            //        await next(context);
            //    }
            //    else
            //    {
            //        if (ApiUrlList.Any(c => c.Equals(context.Request.Path.ToString(), StringComparison.CurrentCultureIgnoreCase)))//接口流量控制启动
            //        {
            //            _apiLog.LogInformation($"流控策略执行中，ApiUrlList：{ApiUrlList.Count}");
            //            await FlowControlCheck(context, next, appAuth, apiInfo, oneTime);
            //        }
            //        else
            //        {
            //            _apiLog.LogDebug($"流控策略结束，请求url {context.Request.Path}不在api列表中");
            //            long flow = await GetResponseBytes(context, next);
            //            apiInfo.StatusCode = context.Response.StatusCode;
            //            apiInfo.Successed = true;
            //            if (context.Response.StatusCode != 200)
            //            {
            //                apiInfo.Successed = false;
            //            }
            //            context.Response.OnCompleted(() =>
            //            {
            //                DateTime twoTime = DateTime.Now;
            //                var consumTime = (twoTime - oneTime).TotalMilliseconds;
            //                apiInfo.ConsumTime = consumTime;
            //                apiInfo.DownBytes = flow;
            //                _sentinelCollection.AddApiInfo(apiInfo);
            //                return Task.CompletedTask;
            //            });
            //        }
            //    }
            //}
            //else
            //{
            //    apiInfo.ApiName = context.Request.Path.ToString();//记录接口信息
            //    apiInfo.CreateTime = oneTime;
            //    apiInfo.UpBytes = await GetRequestBytes(context);//记录接口上行数据
            //    apiInfo.Ip = context.Connection.RemoteIpAddress.MapToIPv4().ToString();//获取IP
            //    long flow = await GetResponseBytes(context, next);
            //    apiInfo.StatusCode = context.Response.StatusCode;
            //    apiInfo.Successed = true;
            //    if (context.Response.StatusCode != 200)
            //    {
            //        apiInfo.Successed = false;
            //    }
            //    context.Response.OnCompleted(() =>
            //    {
            //        DateTime twoTime = DateTime.Now;
            //        var consumTime = (twoTime - oneTime).TotalMilliseconds;
            //        apiInfo.ConsumTime = consumTime;
            //        apiInfo.DownBytes = flow;
            //        _sentinelCollection.AddApiInfo(apiInfo);
            //        return Task.CompletedTask;
            //    });
            //}
            await Task.CompletedTask;
        }

        /// <summary>
        /// 流量控制
        /// </summary>
        //public async Task FlowControlCheck(HttpContext context, RequestDelegate next, IoTNorthAuth appAuth, ApiFlow apiInfo, DateTime oneTime)
        //{
        //    if (_memoryCache.Get(appAuth.AppId) == null)
        //    {
        //        AppTimeInfoCache timeInfoCache = new AppTimeInfoCache()
        //        {
        //            MinuteCountTime = DateTime.Now,
        //            HourCountTime = DateTime.Now,
        //            DayCountTime = DateTime.Now,
        //            MinuteCount = 0,
        //            HourCount = 0,
        //            DayCount = 0,
        //            MinuteMB = 0,
        //            HourMB = 0,
        //            DayMB = 0,
        //        };
        //        // 设置缓存
        //        _memoryCache.Set(appAuth.AppId, timeInfoCache);
        //    }

        //    // IP白名单校验
        //    var data = _memoryCache.Get<AppTimeInfoCache>(appAuth.AppId);
        //    if (WhiteIpCheck(appAuth.WhiteList, apiInfo.Ip))
        //    {
        //        context.Response.OnCompleted(() =>
        //        {
        //            return Task.CompletedTask;
        //        });
        //    }

        //    // IP黑名单校验
        //    if (BlackIpCheck(appAuth.BlackList, apiInfo.Ip))
        //    {
        //        if (context.Response.HasStarted)
        //        {
        //            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        //            var result = new JsonResult(ResponseModel.GetResponseFaild($"黑名单IP:{apiInfo.Ip}禁止访问"));
        //            context.Response.ContentType = "text/plain; charset=utf-8";
        //            await context.Response.WriteAsync(JsonConvert.SerializeObject(result, Formatting.None));
        //        }
        //    }

        //    // 时间次数限制
        //    if (TimeCheck(appAuth.AppId, appAuth.RequestLimitMinute, data.MinuteCountTime, TimeType.Minute, data.MinuteCount, data, appAuth.FluxConfiguration == 1)
        //        || TimeCheck(appAuth.AppId, appAuth.RequestLimitHour, data.HourCountTime, TimeType.Hour, data.HourCount, data, appAuth.FluxConfiguration == 1)
        //        || TimeCheck(appAuth.AppId, appAuth.RequestLimitDay, data.DayCountTime, TimeType.Day, data.DayCount, data, appAuth.FluxConfiguration == 1))
        //    {
        //        if (!context.Response.HasStarted)
        //        {
        //            context.Response.StatusCode = StatusCodes.Status406NotAcceptable;
        //            context.Response.ContentType = "text/plain; charset=utf-8";
        //            var result = new JsonResult(ResponseModel.GetResponseFaild($"已超过API流控策略-时间次数限制，每分钟请求上限：{appAuth.RequestLimitMinute}，每小时请求上限：{appAuth.RequestLimitHour}，每天请求上限：{appAuth.RequestLimitDay}"));
        //            await context.Response.WriteAsync(JsonConvert.SerializeObject(result, Formatting.None));
        //        }
        //    }

        //    // 数据量大小限制
        //    long flow = await GetResponseBytes(context, next);
        //    if (Flowcheck(appAuth.AppId, appAuth.FluxLimitMinute, data.MinuteCountTime, TimeType.Minute, data.MinuteMB, flow, data, appAuth.FluxConfiguration == 1) || Flowcheck(appAuth.AppId, appAuth.FluxLimitHour, data.HourCountTime, TimeType.Hour, data.HourMB, flow, data, appAuth.FluxConfiguration == 1) || Flowcheck(appAuth.AppId, appAuth.FluxLimitDay, data.DayCountTime, TimeType.Day, data.DayMB, flow, data, appAuth.FluxConfiguration == 1))
        //    {
        //        if (!context.Response.HasStarted)
        //        {
        //            context.Response.StatusCode = StatusCodes.Status406NotAcceptable;
        //            context.Response.ContentType = "text/plain; charset=utf-8";
        //            var result = new JsonResult(ResponseModel.GetResponseFaild($"已超过API流控策略-流量控制限制，每分钟请求上限：{appAuth.FluxLimitMinute}，每小时请求上限：{appAuth.FluxLimitHour}，每天请求上限：{appAuth.FluxLimitDay}"));
        //            await context.Response.WriteAsync(JsonConvert.SerializeObject(result, Formatting.None));
        //        }

        //        flow = 0;
        //    }

        //    apiInfo.DownBytes = flow;
        //    apiInfo.StatusCode = context.Response.StatusCode;
        //    apiInfo.Successed = true;
        //    if (context.Response.StatusCode != 200)
        //    {
        //        apiInfo.Successed = false;
        //    }

        //    context.Response.OnCompleted(() =>
        //    {
        //        DateTime twoTime = DateTime.Now;
        //        var consumTime = (twoTime - oneTime).TotalMilliseconds;//计算耗时
        //        apiInfo.ConsumTime = consumTime;
        //        _sentinelCollection.AddApiInfo(apiInfo);
        //        return Task.CompletedTask;
        //    });
        //}

        /// <summary>
        /// IP白名单
        /// </summary>
        /// <param name="whiteList"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public bool WhiteIpCheck(string whiteList, string IP)
        {
            var active = Convert.ToBoolean(_configuration.GetSection("BlackWhiteListMiddleware:active").Value);
            if (!active)
            {
                return false;
            }

            var whiteIpList = _configuration.GetSection("BlackWhiteListMiddleware:WhiteIpList").Value;
            if (string.IsNullOrEmpty(whiteList) && string.IsNullOrEmpty(whiteIpList))
            {
                return false;
            }

            var a = (bool)whiteList?.Split(',').Any(s => s == IP);
            var b = (bool)whiteIpList?.Split(',').Any(s => s == IP);

            if (a || b)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// IP黑名单
        /// </summary>
        /// <param name="blackList"></param>
        /// <param name="IP"></param>
        /// <returns></returns>
        public bool BlackIpCheck(string blackList, string IP)
        {
            var active = Convert.ToBoolean(_configuration.GetSection("BlackWhiteListMiddleware:active").Value);
            if (!active)
            {
                return false;
            }

            var blackIpList = _configuration.GetSection("BlackWhiteListMiddleware:BlackIpList").Value;
            if (string.IsNullOrEmpty(blackList) && string.IsNullOrEmpty(blackIpList))
            {
                return false;
            }

            var a = (bool)blackList?.Split(',').Any(s => s == IP);
            var b = (bool)blackIpList?.Split(',').Any(s => s == IP);
            if (a || b)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 时间次数限制
        /// </summary>
        /// <returns></returns>
        public bool TimeCheck(string appId, long limitCount, DateTime dateTime, TimeType timeType, long count, AppTimeInfoCache data, bool open = false)
        {
            if (!open)
            {
                return false;
            }

            _apiLog.LogInformation($"时间次数限制，count：{count}，limitCount：{limitCount}");
            bool overtime = false;
            switch (timeType)
            {
                case TimeType.Hour: overtime = (DateTime.Now - dateTime).TotalMinutes >= 60; break;
                case TimeType.Minute: overtime = (DateTime.Now - dateTime).TotalSeconds >= 60; break;
                case TimeType.Day: overtime = (DateTime.Now - dateTime).TotalDays >= 1; break;
                default: break;
            };
            if (overtime)
            {
                switch (timeType)
                {
                    case TimeType.Hour: data.HourCountTime = DateTime.Now; data.HourCount = 1; data.HourMB = 0; break;
                    case TimeType.Minute: data.MinuteCountTime = DateTime.Now; data.MinuteCount = 1; data.MinuteMB = 0; break;
                    case TimeType.Day: data.DayCountTime = DateTime.Now; data.DayCount = 1; data.DayMB = 0; break;
                    default: break;
                }
                _memoryCache.Set(appId, data);
                return false;
            }
            else
            {
                if ((count + 1) <= limitCount)
                {
                    switch (timeType)
                    {
                        case TimeType.Hour: data.HourCount++; break;
                        case TimeType.Minute: data.MinuteCount++; break;
                        case TimeType.Day: data.DayCount++; break;
                        default: break;
                    }
                    _memoryCache.Set(appId, data);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// 数据量大小限制
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="limitflow"></param>
        /// <param name="dateTime"></param>
        /// <param name="timeType"></param>
        /// <param name="lastflow"></param>
        /// <param name="flow"></param>
        /// <param name="data"></param>
        /// <param name="open"></param>
        /// <returns></returns>
        public bool Flowcheck(string appId, double limitflow, DateTime dateTime, TimeType timeType, long lastflow, long flow, AppTimeInfoCache data, bool open = false)
        {
            if (!open)
            {
                return false;
            }
            bool overtime = false;
            switch (timeType)
            {
                case TimeType.Hour: overtime = (DateTime.Now - dateTime).TotalMinutes >= 60; break;
                case TimeType.Minute: overtime = (DateTime.Now - dateTime).TotalSeconds >= 60; break;
                case TimeType.Day: overtime = (DateTime.Now - dateTime).TotalDays >= 1; break;
                default: break;
            };
            if (overtime)
            {
                switch (timeType)
                {
                    case TimeType.Hour: data.HourCountTime = DateTime.Now; data.HourCount = 0; data.HourMB = flow; break;
                    case TimeType.Minute: data.MinuteCountTime = DateTime.Now; data.MinuteCount = 0; data.MinuteMB = flow; break;
                    case TimeType.Day: data.DayCountTime = DateTime.Now; data.DayCount = 0; data.DayMB = flow; break;
                    default: break;
                }
                _memoryCache.Set(appId, data);
                return false;
            }
            else
            {
                if ((flow + lastflow) <= limitflow)
                {
                    switch (timeType)
                    {
                        case TimeType.Hour: data.HourMB += flow; break;
                        case TimeType.Minute: data.MinuteMB += flow; break;
                        case TimeType.Day: data.DayMB += flow; break;
                        default: break;
                    }
                    _memoryCache.Set(appId, data);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// 计算上行数据量
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<long> GetRequestBytes(HttpContext context)
        {
            context.Request.EnableBuffering();
            var requestReader = new StreamReader(context.Request.Body);
            var requestContent = await requestReader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            return requestContent.Length;
        }

        /// <summary>
        /// 计算下行数据量
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public static async Task<long> GetResponseBytes(HttpContext context, RequestDelegate next)
        {
            var originalResponseStream = context.Response.Body;
            using (var ms = new MemoryStream())
            {
                context.Response.Body = ms;
                if (!context.Response.HasStarted)
                {
                    await next(context);
                }
                ms.Position = 0;
                using StreamReader sr = new StreamReader(ms);
                var responseReader = sr;
                var responseContent = responseReader.ReadToEnd();
                ms.Position = 0;
                await ms.CopyToAsync(originalResponseStream);
                context.Response.Body = originalResponseStream;
                return responseContent.Length;
            }
        }
    }

    public static class FlowControlMiddlewareExtensions
    {
        public static IApplicationBuilder UseFlowControl(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FlowControlMiddleware>();
        }
    }
}
