using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDemo2.Service
{
    public class PermissionCache
    {
        private static readonly ConcurrentDictionary<string, Info> LoginInfo = new ConcurrentDictionary<string, Info>();

        /// <summary>
        /// 用户登录，或者修改了密码等信息
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static string UserLogin(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }

            string guid = Guid.NewGuid().ToString();

            if (LoginInfo.TryGetValue(userName, out var tmp))
            {
                tmp.Guid = guid;
            }
            else
            {
                var item = new Info()
                {
                    UserName = userName,
                    Guid = guid
                };
                LoginInfo.TryAdd(userName, item);
            }
            return guid;
        }


        /// <summary>
        /// 授权验证
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="gid"></param>
        /// <returns></returns>
        public static bool HavLogin(string userName, string gid)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(gid))
            {
                return false;
            }

            if (LoginInfo.TryGetValue(userName, out var tmp))
            {
                return gid != tmp.Guid;
            }
            return false;

        }

        private class Info
        {
            public string UserName { get; set; }
            public string Guid { get; set; }
        }

    }
}
