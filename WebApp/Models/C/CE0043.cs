using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CE0043
    {
        IEnumerable<CE0043Data> Datas { get; set; }
        IEnumerable<CE0043Count> Count { get; set; }
    }

    public class CE0043Count
    {
        // 盤總
        public string TOT1 { get; set; }    // 品項現品量總金額
        public string TOT2 { get; set; }    // 品項電腦量總金額
        public string TOT3 { get; set; }    // 品項誤差百分比
        public string TOT4 { get; set; }    // 品項誤差總金額
        public string TOT5 { get; set; }    // 品項當季耗總金額
        // 盤盈
        public string P_TOT1 { get; set; }  // 品項現品量總金額
        public string P_TOT2 { get; set; }  // 品項電腦量總金額
        public string P_TOT3 { get; set; }  // 品項誤差百分比
        public string P_TOT4 { get; set; }  // 品項誤差總金額
        public string P_TOT5 { get; set; }  // 品項當季耗總金額
        // 盤虧
        public string N_TOT1 { get; set; }  // 品項現品量總金額
        public string N_TOT2 { get; set; }  // 品項電腦量總金額
        public string N_TOT3 { get; set; }  // 品項誤差百分比
        public string N_TOT4 { get; set; }  // 品項誤差總金額
        public string N_TOT5 { get; set; }  // 品項當季耗總金額
    }
    public class CE0043Data : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }      // 院內碼
        public string MMNAME { get; set; }      // 藥品名稱
        public string STORE_QTY { get; set; }   // 庫存數量
        public string CHK_QTY { get; set; }     // 階段的盤點量
        public string DIFF_QTY { get; set; }    // 誤差量
        public string M_CONTPRICE { get; set; } // 移動平均價
        public string DIFF_AMOUNT { get; set; } // 誤差量金額
        public string DIFF_P { get; set; }      // 差異百分比, 取小數5位
        public string APL_OUTQTY { get; set; }  // 消耗量
        public string DIFF_REMARK { get; set; } // 備註



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



        public string CHK_CLASS { get; set; }
        public string CHK_CLASS_NAME { get; set; }
        public string CONSUME_QTY { get; set; }
        public string CONSUME_AMOUNT { get; set; }

        public string IS_CURRENT_YM { get; set; }
    }
}