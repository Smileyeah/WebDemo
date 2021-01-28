using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDemo2.Models
{

    /// <summary>
    /// 返回对象
    /// </summary>
    public class ResponseModel
    {
        /// <summary>
        /// 返回代码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public dynamic Data { get; set; }


        #region



        /// <summary>
        /// 对象为空时提示
        /// </summary>
        /// <returns></returns>
        public static ResponseModel GetNullInstance(string message = null)
        {
            return new ResponseModel
            {
                Code = 0,
                Message = "传输的参数对象不能为空" + (string.IsNullOrWhiteSpace(message) ? string.Empty : "," + message)
            };
        }

        /// <summary>
        /// 对象为空时提示
        /// </summary>
        /// <returns></returns>
        public static JsonResult JsonResultNullInstance(string message = null)
        {
            return new JsonResult(new ResponseModel
            {
                Code = 0,
                Message = "传输的参数对象不能为空" + (string.IsNullOrWhiteSpace(message) ? string.Empty : "," + message)
            });
        }
        /// <summary>
        /// 当数据或记录已存在或不存在时
        /// </summary>
        /// <param name="state">存在或不存在</param>
        /// <returns></returns>
        public static ResponseModel GetExistsInsrance(bool state)
        {
            return new ResponseModel
            {
                Code = 0,
                Message = state ? "数据或记录已存在" : "数据或记录不存在"
            };
        }
        /// <summary>
        /// 当数据或记录已存在或不存在时
        /// </summary>
        /// <param name="state">存在或不存在</param>
        /// <returns></returns>
        public static JsonResult JsonResultExistsInsrance(bool state)
        {
            return new JsonResult(new ResponseModel
            {
                Code = 0,
                Message = state ? "数据或记录已存在" : "数据或记录不存在"
            });
        }
        /// <summary>
        /// 添加数据时
        /// </summary>
        /// <param name="state">成功或失败</param>
        /// <returns></returns>
        public static ResponseModel GetAddInsrance(bool state)
        {
            return new ResponseModel
            {
                Code = state ? 200 : 0,
                Message = state ? "添加数据或记录成功" : "添加数据或记录失败"
            };
        }

        public static ResponseModel GetAddInsrance(bool state, int id)
        {
            return new ResponseModel
            {
                Code = state ? 200 : 0,
                Message = state ? "添加数据或记录成功" : "添加数据或记录失败",
                Data = new
                {
                    id = id
                }
            };
        }

        /// <summary>
        /// 添加数据时
        /// </summary>
        /// <param name="state">成功或失败</param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResponseModel GetAddInsrance(bool state, object data)

        {
            return new ResponseModel
            {
                Code = state ? 200 : 0,
                Message = state ? "添加数据或记录成功" : "添加数据或记录失败",
                Data = data
            };
        }
        /// <summary>
        /// 添加数据时
        /// </summary>
        /// <param name="state">成功或失败</param>
        /// <returns></returns>
        public static JsonResult JsonResultAddInsrance(bool state)
        {
            return new JsonResult(new ResponseModel
            {
                Code = state ? 200 : 0,
                Message = state ? "添加数据或记录成功" : "添加数据或记录失败"
            });
        }



        public static JsonResult JsonResultAddInsrance(bool state, int id)
        {
            return new JsonResult(new ResponseModel
            {
                Code = state ? 200 : 0,
                Message = state ? "添加数据或记录成功" : "添加数据或记录失败",
                Data = new
                {
                    id = id
                }
            });
        }


        /// <summary>
        /// 修改数据时
        /// </summary>
        /// <param name="state">成功或失败</param>
        /// <returns></returns>
        public static ResponseModel GetEditInsrance(bool state)
        {
            return new ResponseModel
            {
                Code = state ? 200 : 0,
                Message = state ? "修改数据或记录成功" : "修改数据或记录失败"
            };
        }
        public static JsonResult JsonResultEditInsrance(bool state)
        {
            return new JsonResult(new ResponseModel
            {
                Code = state ? 200 : 0,
                Message = state ? "修改数据或记录成功" : "修改数据或记录失败"
            });
        }
        /// <summary>
        /// 删除数据时
        /// </summary>
        /// <param name="state">成功或失败</param>
        /// <returns></returns>
        public static ResponseModel GetDeleteInsrance(bool state)
        {
            return new ResponseModel
            {
                Code = state ? 200 : 0,
                Message = state ? "删除数据或记录成功" : "删除数据或记录失败"
            };
        }
        /// <summary>
        /// 删除数据时
        /// </summary>
        /// <param name="state">成功或失败</param>
        /// <returns></returns>
        public static JsonResult JsonResultDeleteInsrance(bool state)
        {
            return new JsonResult(new ResponseModel
            {
                Code = state ? 200 : 0,
                Message = state ? "删除数据或记录成功" : "删除数据或记录失败"
            });
        }

        /// <summary>
        /// 请求成功
        /// </summary>
        /// <returns></returns>
        public static ResponseModel GetResponseSuccess(string message)
        {
            return new ResponseModel
            {
                Code = 200,
                Message = string.IsNullOrEmpty(message) ? "success" : message
            };
        }

        /// <summary>
        /// 请求成功
        /// </summary>
        /// <returns></returns>
        public static JsonResult JsonResultResponseSuccess(string message)
        {
            return new JsonResult(new ResponseModel
            {
                Code = 200,
                Message = string.IsNullOrEmpty(message) ? "success" : message
            });
        }

        /// <summary>
        /// 请求失败
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ResponseModel GetResponseFaild(string message)
        {
            return new ResponseModel
            {
                Code = 0,
                Message = message
            };
        }
        /// <summary>
        /// 请求失败
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static JsonResult JsonResultResponseFaild(string message)
        {
            return new JsonResult(new ResponseModel
            {
                Code = 0,
                Message = message
            });
        }
        /// <summary>
        /// 获取数据成功
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResponseModel GetResultSuccess(dynamic data)
        {
            return new ResponseModel
            {
                Code = 200,
                Message = "success",
                Data = data
            };
        }

        /// <summary>
        /// 获取结果成功
        /// </summary>
        /// <param name="Total">总数</param>
        /// <param name="pageNo">第几页</param>
        /// <param name="pageSize">每页多少个</param>
        /// <param name="list">数据列表</param>
        /// <returns></returns>
        public static ResponseModel GetResultSuccess(int Total, int pageNo, int pageSize, dynamic list)
        {
            return new ResponseModel
            {
                Code = 200,
                Message = "success",
                Data = new
                {
                    Total,
                    pageNo,
                    pageSize,
                    List = list
                }
            };
        }

        public static ResponseModel GetResultSuccess(int Total, int pageNo, int pageSize, dynamic list, dynamic other)
        {
            return new ResponseModel
            {
                Code = 200,
                Message = "success",
                Data = new
                {
                    Total,
                    pageNo,
                    pageSize,
                    Other = other,
                    List = list
                }
            };
        }
        /// <summary>
        /// 获取数据成功
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static JsonResult JsonResultResultSuccess(dynamic data)
        {
            return new JsonResult(new ResponseModel
            {
                Code = 200,
                Message = "success",
                Data = data
            });
        }

        /// <summary>
        /// 获取结果成功
        /// </summary>
        /// <param name="Total">总数</param>
        /// <param name="list">数据列表</param>
        /// <returns></returns>
        public static JsonResult JsonResultResultSuccess(int Total, dynamic list)
        {
            return new JsonResult(new ResponseModel
            {
                Code = 200,
                Message = "success",
                Data = new
                {
                    Total,
                    List = list
                }
            });
        }

        /// <summary>
        /// 获取结果成功
        /// </summary>
        /// <param name="Total">总数</param>
        /// <param name="pageNo">第几页</param>
        /// <param name="pageSize">每页多少个</param>
        /// <param name="list">数据列表</param>
        /// <returns></returns>
        public static JsonResult JsonResultResultSuccess(int Total, int pageNo, int pageSize, dynamic list)
        {
            return new JsonResult(new ResponseModel
            {
                Code = 200,
                Message = "success",
                Data = new
                {
                    Total,
                    pageNo,
                    pageSize,
                    List = list
                }
            });
        }

        public static JsonResult Get403(string message)
        {
            return new JsonResult(new ResponseModel
            {
                Code = 0,
                Message = message
            });
        }

        #endregion
    }

    /// <summary>
    /// ResponseModel 拓展函数
    /// </summary>
    public static class ResponseModelExtension
    {

    }
}
