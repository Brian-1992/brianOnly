using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class LoginUserResult
    {
        /// <summary>
        /// 此次呼叫 API 是否成功
        /// </summary>
        public string Result { get; set; } = "1";
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 部門
        /// </summary>
        public string Deptno { get; set; } = "";
        /// <summary>
        /// 額外訊息
        /// </summary>
        public string Message { get; set; } = "";
    }
}