using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class ApiResult
    {
        /// <summary>
        /// 查詢結果總筆數
        /// </summary>
        public int? TotalCount { get; set; }

        public object Data { get; set; }

        public string Msg { get; set; }

        public bool? Success { get; set; }



    }
}