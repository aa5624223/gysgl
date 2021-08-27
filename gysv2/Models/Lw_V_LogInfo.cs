using System;

namespace gysv2.Models
{
    public class Lw_V_LogInfo
    {
        #region 属性

        /// <summary>
        /// 主键
        /// </summary>
        [Property(PType="PK")]
        public long ID { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        [Property(Model = 1)]
        public string TypeName { get; set; }

        /// <summary>
        /// 操作内容
        /// </summary>
        [Property()]
        public string Contents { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        [Property()]
        public DateTime DateTime1 { get; set; }

        public string Datetime10 { get; set; }

        public string Datetime11 { get; set; }

        /// <summary>
        /// 用户编码
        /// </summary>
        [Property()]
        public string UserCode { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Property(Model = 1)]
        public string UserName { get; set; }

        #endregion
    }
}
