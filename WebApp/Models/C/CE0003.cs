using System;
using System.Collections.Generic;

namespace WebApp.Models.C
{
    public class CE0003 : JCLib.Mvc.BaseModel
    {
        //以下是 CE0003
        public string CHK_WH_NO { get; set; }
        public string WH_NAME { get; set; }
        public string CHK_NO { get; set; }
        public string CHK_YM { get; set; }
        public string CHK_WH_GRADE { get; set; }
        public string CHK_WH_KIND { get; set; }
        public string CHK_PERIOD { get; set; }
        public string CHK_TYPE { get; set; }
        public string MERGE_NUM_TOTAL { get; set; }
        public string CHK_KEEPER { get; set; }
        public string CHK_STATUS { get; set; }

        //以下是 CE0003_INI
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTPRICE { get; set; }
        public string WH_NO { get; set; }
        public string STORE_LOC { get; set; } // 儲位 
        public string LOC_NAME { get; set; }
        public string MAT_CLASS { get; set; }
        public string M_STOREID { get; set; }
        public string STORE_QTYC { get; set; } // 電腦量 
        public string STORE_QTYM { get; set; }
        public string STORE_QTYS { get; set; }
        public string CHK_QTY { get; set; }// 盤點量 
        public string CHK_REMARK { get; set; } // 備註
        public string CHK_UID { get; set; }
        public string UPDN_STATUS { get; set; }

        public DateTime? CHK_TIME { get; set; }
        public string STATUS_INI { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }


        public string CHK_WH_GRADE_CODE { get; set; }
        public string CHK_WH_KIND_CODE { get; set; }
        public string CHK_PERIOD_CODE { get; set; }
        public string CHK_TYPE_CODE { get; set; }
        public string CHK_STATUS_CODE { get; set; }
        public string NOSIGN { get; set; }
        public string SIGN { get; set; }
        public string CHK_TOTAL { get; set; }

        public string DIFF_QTY { get; set; }

        public string CHK_UID_NAME { get; set; }
        public string CHK_TYPE_NAME { get; set; }
        public string CHK_CLASS_NAME { get; set; }
        public string CHK_STATUS_NAME { get; set; }
        public string PRE_INV_QTY { get; set; } // 本月期初量 = 上月結存
        public string APL_INQTY { get; set; }   // 本月入庫量
        public string APL_OUTQTY { get; set; }   // 本月出庫量
        public string M_TRNID { get; set; }     // 扣庫方式(1扣庫2不扣庫)
        public string CHK_UID_COUNT { get; set; }
        public string USE_QTY { get; set; } // 前端名稱: 批價扣庫、資料庫: 耗用總量
        public string HIS_CONSUME_QTY_T { get; set; }
        public string HIS_CONSUME_DATATIME { get; set; }
        public string ORI_STORE_QTYC { get; set; }
        public string ORI_USE_QTY { get; set; }
        public string BACK_QTY { get; set; }
        public string BAK_INQTY { get; set; }
        public string BAK_OUTQTY { get; set; }
        public string TRN_INQTY { get; set; }
        public string TRN_OUTQTY { get; set; }
        public string ADJ_INQTY { get; set; }
        public string ADJ_OUTQTY { get; set; }
        public string CHK_NO1 { get; set; }
        public string CHK_NO1_CREATE_USER { get; set; }
        public string CONSUME_QTY { get; set; }
        public string ALTERED_USE_QTY { get; set; } // 調整消耗 
        public string HID_ALTERED_USE_QTY { get; set; }
        public string INVENTORY { get; set; } // 差異量
        public string DIFF_PRICE { get; set; } // 差異金額
        public string PYM_INV_QTY { get; set; } // 上期結存
        public string REJ_OUTQTY { get; set; } // 退貨量 
        public string DIS_OUTQTY { get; set; } // 報廢量 
        public string exg_inqty { get; set; } // 換貨入 
        public string EXG_OUTQTY { get; set; } // 換貨出 
        public string DISC_CPRICE { get; set; } // 優惠後單價
        public string G34_MAX_APPQTY { get; set; } // 單位請領基準量
        public string EST_APPQTY { get; set; } // 下月預估申請量

        public string CHK_TIME_STRING { get; set; }

        //CE0041
        public string THEORY_QTY { get; set; }
        public string BASE_QTY_45 { get; set; } //45天基準量 (805專用)

        public string ORI_CHK_QTY { get; set; }

        public string SET_CTIME { get; set; }
    }
}
