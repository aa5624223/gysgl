using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gysv2.Models
{
    public class V_Lw_Msg
    {

        [Property(PType = "PK", Model = 3)]
        public int ID
        {
            get
            ;
            set
            ;
        }

        /// <summary>
        /// 消息内容
        /// </summary>
        [Property()]
        public string Msg
        {
            get
            ;
            set
            ;
        }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Property(Model = 3)]
        public int UserID
        {
            get
            ;
            set
            ;
        }

        /// <summary>
        /// 供应商ID
        /// </summary>
        [Property(Model = 3)]
        public int GysID
        {
            get
            ;
            set
            ;
        }

        /// <summary>
        /// 附件 ,间隔
        /// </summary>

        [Property()]
        public string Files
        {
            get
            ;
            set
            ;
        }
        /// <summary>
        /// 发送时间
        /// </summary>

        [Property()]
        public DateTime SendeTime
        {
            get
            ;
            set
            ;
        }

        /// <summary>
        /// 接收时间
        /// </summary>
        [Property()]
        public DateTime ReceiveTime
        {
            get
            ;
            set
            ;
        }

        /// <summary>
        /// 发送方是谁
        /// user:用户 Gys:供应商
        /// </summary>
        [Property()]
        public string MsgType
        {
            get
            ;
            set
            ;
        }

        /// <summary>
        /// 供应商名
        /// </summary>
        [Property()]
        public string GysName
        {
            get
            ;
            set
            ;
        }

        /// <summary>
        /// 用户名
        /// </summary>
        [Property()]
        public string UserName
        {
            get
            ;
            set
            ;
        }

        /// <summary>
        /// 对方读取的时间
        /// </summary>
        [Property()]
        public DateTime ReadTime
        {
            get
            ;
            set
            ;
        }

        /// <summary>
        /// 对方是否读取
        /// </summary>
        [Property(Model = 3)]
        public int isRead
        {
            get
            ;
            set
            ;
        }

    }
}