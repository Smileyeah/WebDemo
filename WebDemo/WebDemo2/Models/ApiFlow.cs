using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDemo2.Models
{
    public enum TimeType
    {
        Minute = 0,
        Hour = 1,
        Day = 2
    }

    public class ApiFlow
    {
        public long Id { get; set; }//主键
        public string ApiName { get; set; }//接口名称
        public bool Successed { get; set; }//是否失败
        public int StatusCode { get; set; }//出错编号
        public long UpBytes { get; set; }//上行访问数据量
        public long DownBytes { get; set; }//下行访问数据量
        public double ConsumTime { get; set; }//计算耗时
        public string Ip { get; set; }//访问IP
        public DateTime CreateTime { get; set; }//访问时间
    }

    public class AppTimeInfoCache
    {
        public DateTime MinuteCountTime { get; set; }
        public DateTime HourCountTime { get; set; }
        public DateTime DayCountTime { get; set; }
        public long MinuteMB { get; set; }
        public long HourMB { get; set; }
        public long DayMB { get; set; }
        public long MinuteCount { get; set; }//每分钟内获取的次数
        public long HourCount { get; set; }//每小时内获取的次数
        public long DayCount { get; set; }//每天内获取的次数
    }
}
