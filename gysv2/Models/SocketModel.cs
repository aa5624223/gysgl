using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gysv2.Models
{
    /// <summary>
    /// 用于存放Soeckt
    /// </summary>
    public class SocketModel
    {
        /// <summary>
        /// 指定改Socket属于哪个用户
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 目标是供应商 还是用户
        /// 供应商 gys 用户user
        /// </summary>
        public string UserType { get; set; }

        public string SocketID { get; set; }

        

        /// <summary>
        /// 当前用户分配的Socekt
        /// </summary>
        public IWebSocketConnection Socket { get; set; }
    }
}