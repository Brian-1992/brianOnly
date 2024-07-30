using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    /// <summary>
    /// 呼叫 API 回傳格式
    /// </summary>
    public class LoginAPIResult
    {
        /// <summary>
        /// 此次呼叫 API 是否成功
        /// </summary>
        public string Result { get; set; } = "1";
        /// <summary>
        /// 呼叫 API 失敗的錯誤訊息
        /// </summary>
        public string Message { get; set; } = "";
        /// <summary>
        /// 呼叫此API所得到的其他內容
        /// </summary>
        public object Token { get; set; }
    }
}