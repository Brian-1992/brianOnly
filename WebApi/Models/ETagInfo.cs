using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class ETagInfo
    {
        public string WH_NO { get; set; }       // 庫房代碼
        public IEnumerable<string> MMCODES { get; set; }        //院內碼清單
        public string MMCODE { get; set; }      // 院內碼
        public string INV_QTY { get; set; }     // 現存量
        public string MMNAME_E { get; set; }        // 英文品名
        public string MMNAME_C { get; set; }        // 中文品名
        public string BASE_UNIT { get; set; }       // 計量單位
    }
}