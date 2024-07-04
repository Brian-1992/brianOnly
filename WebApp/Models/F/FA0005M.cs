using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class FA0005M : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }      // 院內馬
        public string MMNAME_C { get; set; }    // 中文品名
        public string MMNAME_E { get; set; }    // 英文品名
        public string STORE_LOC { get; set; }   // 儲位
        public string CHK_REMARK { get; set; }  // 說明
        public string STATUS_TOT { get; set; }  // 盤點期
        public string STORE_QTY { get; set; }
        public string M_CONTPRICE { get; set; }
        public string STORE_COST { get; set; }
        public string CHK_QTY { get; set; }     // 實盤數量
        public string CHK_COST { get; set; }    // 實盤成本
        public string CHK_QTY1 { get; set; }    // 初盤量
        public string CHK_QTY2 { get; set; }    // 複盤量
        public string CHK_QTY3 { get; set; }    // 三盤量
        public string APL_OUTQTY { get; set; }  // 申請出庫量
        public string COMMENT { get; set; }     // 差異說明(儲位 + 說明) 報表用
        public string DIFF_QTY { get; set; }    // 差異數量
        public string DIFF_COST { get; set; }   // 差異成本
        public string DIFF_PERC { get; set; }   // 差異百分率
        public string PRINTUSER { get; set; }
        public string CHK_YYY { get; set; }
        public string CHK_MM { get; set; }
        public IEnumerable<FA0005M> items { get; set; }
    }
}