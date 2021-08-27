using System;
using System.Collections.Generic;
using System.Text;

namespace gysv2.Models
{
    /// <summary>
    /// 消息类
    /// 用于回传给客户端的实体类
    /// </summary>
    class MsgInfo
    {
        /// <summary>
        /// 200 正确
        /// 400 客户端传值问题
        /// 404 数据未找到等问题
        /// 500 服务器错误
        /// 其他 ""
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 状态码Code的详解
        /// </summary>
        public string Msg { get {
                switch (Code)
                {
                    case 200:
                        return "OK";
                    case 400:
                        return "Bad Request";
                    case 404:
                        return "Not Fount";
                    case 500:
                        return "Server Error";
                    default:
                        return "";
                }
            } 
        }

        /// <summary>
        /// 问题详细
        /// </summary>
        public string Info { get; set; }

    }
}
