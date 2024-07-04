using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0156 : JCLib.Mvc.BaseModel
    {
        public string APPQTY { get; set; }        // 數量
        public string UP { get; set; }            // 單價
        public string AMT { get; set; }            // 金額
        public string MMCODE { get; set; }           // 院內碼
        public string MMNAME_C { get; set; }          // 中文品名
        public string MMNAME_E { get; set; }           // 英文品名
        public string BASE_UNIT { get; set; }       // 計量單位
    }
}