using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gysv2.Models
{
    public class Property : Attribute
    {
        public Property()
        {

        }

        /// <summary>
        /// 主键=PK,外键=FK
        /// </summary>
        public string PType { get; set; }

        /// <summary>
        /// 匹配类型
        /// 1.String
        /// 0:精确匹配 1:模糊匹配 2:in 匹配
        /// 2.数字
        /// 0:精确匹配 1:小于等于 2:大于等于 3:不匹配
        /// 值为-1 不进行匹配
        /// </summary>
        public int Model { get; set; }

        /// <summary>
        /// 当string
        /// Model:2时,选这个做字段
        /// </summary>
        public string Colums { get; set; }

        /// <summary>
        /// 0.普通字符串
        /// 1.时间类字符串
        /// </summary>
        public int StringModel { get; set; }

        /// <summary>
        /// 不加入到select字段内
        /// </summary>
        public bool NotSelect { get; set; }

    }
}