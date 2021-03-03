using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebDemo2.Filter
{
    public class CustomFilterAttribute : ActionFilterAttribute
    {
        private static Regex SqlTokenPattern { get; set; }
        private static StringBuilder PatternString { get; set; } = new StringBuilder();

        private const string StrKeyWord = @"select |insert |delete |from |count|drop table |update |truncate|asc|mid|char|xp_cmdshell|exec|exec master|netlocalgroup administrators|net user|or |and |like |waitfor delay |sleep";

        private readonly ILogger<CustomFilterAttribute> _apiLog;

        public CustomFilterAttribute(ILogger<CustomFilterAttribute> apiLog)
        {
            _apiLog = apiLog;
        }

        static CustomFilterAttribute()
        {
            string[][] search_regex_replacement = new string[10][];
            search_regex_replacement[0] = new string[] { "\u0000", "\\x00", "\\\\0" };
            search_regex_replacement[1] = new string[] { "'", "'", "\\\\'" };
            search_regex_replacement[2] = new string[] { "\"", "\"", "\\\\\"" };
            search_regex_replacement[3] = new string[] { "\b", "\\x08", "\\\\b" };
            search_regex_replacement[4] = new string[] { "\n", "\\n", "\\\\n" };
            search_regex_replacement[5] = new string[] { "\r", "\\r", "\\\\r" };
            search_regex_replacement[6] = new string[] { "\t", "\\t", "\\\\t" };
            search_regex_replacement[7] = new string[] { "\u001A", "\\x1A", "\\\\Z" };
            search_regex_replacement[8] = new string[] { "\\", "\\\\", "\\\\\\\\" };
            search_regex_replacement[9] = new string[] { "\\%", "\\%", "\\\\'" };

            foreach (var srr in search_regex_replacement)
            {
                PatternString.Append($"{(string.IsNullOrEmpty(PatternString.ToString()) ? "" : "|")}" +
                    $"{srr[1]}");
            }
        }

        //在方法执行前执行过滤器
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                return;
            }

            // 忽略过滤器
            var action = context.ActionDescriptor as ControllerActionDescriptor;
            if (action != null && action.MethodInfo.GetCustomAttributes(typeof(SkipCustomFilterAttribute), false).Any())
            {
                return;
            }

            #region SQL注入校验
            var parameters = context.ActionDescriptor.Parameters
                   .Where(p => context.ActionArguments.ContainsKey(p.Name) && context.ActionArguments[p.Name] != null);

            foreach (var p in parameters)
            {
                string parameterName = p.Name;

                if (p.ParameterType == typeof(string))//如果参数是str类型
                {
                    string value = context.ActionArguments[parameterName].ToString();
                    // 开始检查请求参数值是否合法
                    if (Validate(value) || CheckKeyWord(value))
                    {
                        //返回json格式数数据
                        context.Result = new JsonResult(new { code = 403, message = "请求参数存在非法字符!", data = false });
                        base.OnActionExecuting(context);
                    }
                }
                else if (p.ParameterType.IsClass && p.ParameterType.Name != "List`1")//当参数是一个实体
                {
                    PostModelFieldFilter(context, p.ParameterType, context.ActionArguments[parameterName]);
                }
            }

            base.OnActionExecuting(context);
            #endregion
        }

        /// <summary>
        /// 对实体处理,获取属性的值
        /// </summary>
        /// <returns></returns>
        private void PostModelFieldFilter(ActionExecutingContext context, Type type, object obj)
        {
            if (obj == null)
            {
                return;
            }

            try
            {
                //获取类的属性集合
                var props = type.GetProperties();

                PostModelFieldFilterOne(context, obj, props);
            }
            catch (Exception ex)
            {
                _apiLog.LogError("ErrorRequestFilterAttribute【参数过滤异常】：" + ex.ToString());
            }
        }

        private void PostModelFieldFilterOne(ActionExecutingContext context, object obj, PropertyInfo[] props)
        {
            foreach (var propertyInfo in props)//遍历属性
            {
                if (propertyInfo.Name == "Capacity")
                {
                    break;
                }
                if (propertyInfo.PropertyType.Name == "Dictionary`2")
                    continue;

                //当参数是str
                if (propertyInfo.PropertyType == typeof(string))
                {
                    var value = propertyInfo.GetValue(obj);
                    if (value == null)
                    {
                        continue;
                    }
                    // 开始检查请求参数值是否合法
                    if (Validate(value.ToString()) || CheckKeyWord(value.ToString()))
                    {
                        //返回json格式数数据
                        context.Result = new JsonResult(new { code = 403, message = "请求参数存在非法字符!", data = false });
                        base.OnActionExecuting(context);
                    }
                    else
                    {
                        base.OnActionExecuting(context);
                    }
                }
                else if (propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType.Name != "List`1")//当属性是一个实体
                {
                    PostModelFieldFilter(context, propertyInfo.PropertyType, propertyInfo.GetValue(obj));
                }
            }
        }

        /// <summary>
        /// 检查_sword是否包涵SQL关键字.
        /// </summary>
        /// <param name="sWord">需要检查的字符串.
        /// </param>
        /// <returns>存在SQL注入关键字时返回 true，否则返回 false.
        /// </returns>
#pragma warning disable CA1822 // 将成员标记为 static
        public bool CheckKeyWord(string sWord)
#pragma warning restore CA1822 // 将成员标记为 static
        {
            if (string.IsNullOrEmpty(sWord))
            {
                return false;
            }

            var word = sWord;

            // 模式1 : 对应Sql注入的可能关键字
            var matches = StrKeyWord.Split('|');

            // 开始检查 模式1:Sql注入的可能关键字 的注入情况
            foreach (string matche in matches)
            {
                word = Regex.Replace(sWord.Trim(), matche.Trim(), "", RegexOptions.IgnoreCase);
            }

            return !word.Equals(sWord, StringComparison.OrdinalIgnoreCase);
        }

        private static bool Validate(string text)
        {
            SqlTokenPattern = new Regex(PatternString.ToString(),
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var matches = SqlTokenPattern.Matches(text);

            return matches.Count > 0;
        }
    }
}
