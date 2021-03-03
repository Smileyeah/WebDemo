using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDemo2.Filter
{
    /// <summary>
    /// 忽略全局Sql过滤器
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class SkipCustomFilterAttribute : Attribute
    {
    }
}
