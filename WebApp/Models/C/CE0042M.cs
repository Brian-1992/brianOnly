using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CE0042M : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }      // 院內碼
        public string MMNAME { get; set; }      // 藥品名稱
        public string STORE_QTY { get; set; }   // 庫存數量
        public string STORE_COST { get; set; }    // 庫存成本
        public string CHK_QTY { get; set; }     // 階段的盤點量
        public string CHK_COST { get; set; }    // 階段的盤成本
        public string DIFF_P { get; set; }      // 差異百分比, 取小數5位
        public string DIFF_COST { get; set; }    // 差異成本
        public string DIFF_REMARK { get; set; } // 備註

        public string CONDITION { get; set; }



        public string CHK_NO { get; set; }
        public string CHK_YM { get; set; }
        public string CHK_WH_NO { get; set; }
        public string CHK_WH_GRADE { get; set; }
        public string CHK_WH_KIND { get; set; }
        public string CHK_PERIOD { get; set; }
        public string CHK_TYPE { get; set; }
        public string CHK_LEVEL { get; set; }
        public string CHK_STATUS { get; set; }
        public string CHK_KEEPER { get; set; }
        public string WH_NAME { get; set; }
        public string WH_GRADE_NAME { get; set; }
        public string WH_KIND_NAME { get; set; }
        public string CHK_PERIOD_NAME { get; set; }
        public string CHK_TYPE_NAME { get; set; }
        public string CHK_LEVEL_NAME { get; set; }
        public string CHK_STATUS_NAME { get; set; }
        public string CHK_KEEPER_NAME { get; set; }

        public string CONSUME_QTY { get; set; }
        public string CONSUME_AMOUNT { get; set; }
        public string CHK_CLASS { get; set; }
        public string CHK_CLASS_NAME { get; set; }

        public string IS_CURRENT_YM { get; set; }
    }
}