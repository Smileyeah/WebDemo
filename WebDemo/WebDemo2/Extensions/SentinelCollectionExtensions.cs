using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebDemo2.Models;
using WebDemo2.Service;

namespace WebDemo2.Extensions
{
    public class SentinelCollectionExtensions : IDisposable
    {
        public long TotalVisit { get; set; }//总访问量统计
        private bool isFirst;
        private Timer timer;
        private readonly List<ApiFlow> apiCollectionInfos = new List<ApiFlow>();//Api信息搜集
        private readonly IServiceProvider _serviceProvider;
        public static ILogger<SentinelCollectionExtensions> ApiLog;

        public SentinelCollectionExtensions(IServiceProvider serviceProvider,
            ILogger<SentinelCollectionExtensions> apiLog)
        {
            ApiLog = apiLog;
            _serviceProvider = serviceProvider;
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            timer = new Timer(new TimerCallback((e) =>
            {
                if (!isFirst)
                {
                    isFirst = true;
                    Task.Delay(60000);
                }
                DicToDB();
                timer.Change(60000, Timeout.Infinite);
            }), null, 60000, Timeout.Infinite);
        }

        /// <summary>
        /// api访问信息
        /// </summary>
        public void AddApiInfo(ApiFlow apiInfo)
        {
            ApiLog.LogDebug($"访问地址：{apiInfo.ApiName}访问次数：{TotalVisit}");
            apiCollectionInfos.Add(apiInfo);
        }

        /// <summary>
        /// 将缓存写入数据库
        /// </summary>
        public void DicToDB()
        {
            //using (GWDbContext _dbContext = (GWDbContext)_serviceProvider.CreateScope().ServiceProvider.GetService(typeof(GWDbContext)))
            //{
            //    var newList = new List<ApiFlow>(apiCollectionInfos);
            //    ApiLog.LogDebug($"目前缓存Api信息数:{newList.Count}");
            //    for (var i = 0; i < newList.Count; i++)
            //    {
            //        _dbContext.ApiFlow.Add(newList[i]);
            //    }
            //    if (newList.Count == 0)
            //    {
            //        return;
            //    }
            //    _dbContext.SaveChanges();
            //    apiCollectionInfos.RemoveRange(0, newList.Count);
            //}
        }

        /// <summary>
        /// 获取北向接口基本信息表
        /// </summary>
        /// <returns></returns>
        //public List<IoTNorthApiList> GetAPiList()
        //{
        //    using (GWDbContext _dbContext = (GWDbContext)_serviceProvider.CreateScope().ServiceProvider.GetService(typeof(GWDbContext)))
        //    {
        //        return _dbContext.IoTNorthApiList.ToList();
        //    }
        //}

        internal class GetTokenInput
        {
            public string appId { get; set; }
        }

        /// <summary>
        /// 获取北向接口授权表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ApiId"></param>
        /// <returns></returns>
        //public IoTNorthAuth GetAppId(HttpContext context, string ApiId)
        //{
        //    using (GWDbContext _dbContext = (GWDbContext)_serviceProvider.CreateScope().ServiceProvider.GetService(typeof(GWDbContext)))
        //    {
        //        // 如果是gettoken接口，不存在登陆用户
        //        if (context.Request.Path.ToString().Contains("auth/getToken", StringComparison.OrdinalIgnoreCase))
        //        {
        //            context.Request.EnableBuffering();
        //            context.Request.Body.Position = 0;
        //            using var reader = new StreamReader(context.Request.Body);
        //            var param = reader.ReadToEnd();

        //            var input = JsonSerializer.Deserialize<GetTokenInput>(param, new JsonSerializerOptions()
        //            {
        //                PropertyNameCaseInsensitive = true
        //            });

        //            if (!string.IsNullOrEmpty(input?.appId))
        //            {
        //                return _dbContext.IoTNorthAuth.FirstOrDefault(c => c.AppId == input.appId && c.ApiId == ApiId && c.SubState == 1);
        //            }
        //        }

        //        var token = TokenRetrieval.FromAuthorizationHeader()(context.Request);
        //        if (!string.IsNullOrWhiteSpace(token))
        //        {
        //            var userInfo = token.ReadToken();
        //            ApiLog.LogDebug($"获取鉴权信息，appid：{userInfo.UserName},apiid:{ApiId}");
        //            return _dbContext.IoTNorthAuth.FirstOrDefault(c => c.AppId == userInfo.UserName && c.ApiId == ApiId && c.SubState == 1);
        //        }

        //        return null;
        //    }
        //}

        public void Dispose()
        {
            if (timer != null) timer.Dispose();
        }
    }
}
