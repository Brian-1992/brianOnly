using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CHK_DETAIL : JCLib.Mvc.BaseModel
    {
        public string CHK_NO { get; set; }      // 盤點單號
        public string MMCODE { get; set; }      // 院內碼
        public string MMNAME_C { get; set; }    // 中文品名
        public string MMNAME_E { get; set; }    // 英文品名
        public string M_PURUN { get; set; }     // 申購計量單位
        public string M_CONTPRICE { get; set; }  // 合約單價
        public string WH_NO { get; set; }       // 庫房代碼
        public string WH_NO_C { get; set; }       // 庫房名稱(庫房代碼的中文)
        public string STORE_LOC { get; set; }   // 儲位代碼
        public string LOC_NAME { get; set; }    // 儲位名稱
        public string MAT_CLASS { get; set; }   // 物料分類代碼
        public string M_STOREID { get; set; }   // 庫備識別碼(0非庫備,1庫備)
        public string STORE_QTYC { get; set; }  // 電腦總量(民)
        public string STORE_QTYM { get; set; }  // 電腦總量(軍)
        public string STORE_QTYS { get; set; }  // 電腦總量(院)
        public string CHK_QTY { get; set; }     // 盤點量
        public string CHK_REMARK { get; set; }  // 附註說明
        public string CHK_UID { get; set; }     // 盤點人員代碼
        public string CHK_TIME { get; set; }    // 盤點時間
        public string STATUS_INI { get; set; }  // 狀態(1:開立, 2.盤中, 3:鎖單)
        public string STATUS_INI_NAME { get; set; }  // 狀態(0:開立, 1.盤中, 2:鎖單)
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string STORE_QTY_N { get; set; }
        public string CHK_TIME_T { get; set; }    // 盤點時間
        public string INVENTORY { get; set; }


        //public string MANAGERID { get; set; }   // 衛材找到盤點人員，藥品留空白
        public string CHK_UID_NAME { get; set; }
        public string BASE_UNIT { get; set; }
        public string ITEM_STRING { get; set; }

        public string E_TAKEKIND { get; set; }
        public string E_RESTRICTCODE { get; set; }
        public double INV_QTY { get; set; }
        public string QTY_DIFF { get; set; }

        public string STORE_LOC_NAME { get; set; }

        public IEnumerable<CHK_DETAIL> tempDetails { get; set; }
        public string CHK_NO1 { get; set; }

        public string MISS_PER { get; set; } // 總誤差比
        public string GAP_T { get; set; }   // 總盤差

        public string DIFF_P { get; set; }
        public string NEW_STORE_QTYC { get; set; }

        public string STORE_QTY { get; set; }  // 電腦量
        public string CHK_QTY_DIFF { get; set; }


        public string DONE_NUM { get; set; }
        public string UNDONE_NUM { get; set; }
        public string DONE_STATUS { get; set; }


        public string CHK_TYPE_NAME { get; set; }
        public string CHK_TYPE { get; set; }
        public string CHK_WH_KIND { get; set; }
        public string TYPE_ABBRV { get; set; }

        public string CONSUME_QTY { get; set; }
        public string CONSUME_AMOUNT { get; set; }

        public string PRE_INV_QTY { get; set; }
        public string APL_INQTY { get; set; }
        public string APL_OUTQTY { get; set; }

        public string WH_KIND { get; set; }

        public string SEQ { get; set; }

        public string USE_QTY { get; set; }

        public string MEMO { get; set; }

        public string HIS_CONSUME_QTY_T { get; set; }
        public string HIS_CONSUME_DATATIME { get; set; }

        public string ORI_STORE_QTYC { get; set; }
        public string CHECK_RESULT { get; set; }

        public string ALTERED_USE_QTY { get; set; } // 調整消耗
        public string G34_MAX_APPQTY { get; set; } // 單位請領基準量
        public string EST_APPQTY { get; set; } // 下月預估申請量
        public string PYM_INV_QTY { get; set; }
        public string TRN_INQTY { get; set; }
        public string TRN_OUTQTY { get; set; }
        public string ADJ_INQTY { get; set; }
        public string ADJ_OUTQTY { get; set; }
        public string BAK_INQTY { get; set; }
        public string BAK_OUTQTY { get; set; }
        public string REJ_OUTQTY { get; set; }
        public string DIS_OUTQTY { get; set; }
        public string EXG_INQTY { get; set; }
        public string EXG_OUTQTY { get; set; }
        public string MIL_INQTY { get; set; }
        public string MIL_OUTQTY { get; set; }
        public string MAT_CLASS_SUB { get; set; }
        public string DISC_CPRICE { get; set; }
        public string PYM_CONT_PRICE { get; set; }
        public string PYM_DISC_CPRICE { get; set; }
        public string WAR_QTY { get; set; }
        public string UNITRATE { get; set; }
        public string M_CONTID { get; set; }
        public string E_SOURCECODE { get; set; }
        public string COMMON { get; set; }
        public string FASTDRUG { get; set; }
        public string DRUGKIND { get; set; }
        public string TOUCHCASE { get; set; }
        public string ORDERKIND { get; set; }
        public string SPDRUG { get; set; }

        public bool IsPass { get; set; }

        public string ORI_CHK_QTY { get; set; }
        public string CHK_YM { get; set; }
        public string CHK_WH_GRADE {get;set;}
        public string CHK_WH_NO { get; set; }
        public string WH_NAME { get; set; }
        public string USE_QTY_AF_CHK { get; set; }

        public CHK_DETAIL()
        {
            CHK_NO = string.Empty;
            MMCODE = string.Empty;
            MMNAME_C = string.Empty;
            MMNAME_E = string.Empty;
            M_PURUN = string.Empty;
            M_CONTPRICE = string.Empty;
            STORE_LOC = string.Empty;
            LOC_NAME = string.Empty;
            MAT_CLASS = string.Empty;
            M_STOREID = string.Empty;
            STORE_QTYC = string.Empty;
            STORE_QTYM = string.Empty;
            STORE_QTYS = string.Empty;
            CHK_QTY = string.Empty;
            CHK_REMARK = string.Empty;
            CHK_UID = string.Empty;
            CHK_TIME = string.Empty;
            STATUS_INI = string.Empty;
            STATUS_INI_NAME = string.Empty;
            CHK_UID_NAME = string.Empty;
            BASE_UNIT = string.Empty;
            ITEM_STRING = string.Empty;

            E_TAKEKIND = string.Empty;
            E_RESTRICTCODE = string.Empty;
            INV_QTY = 0;
            STORE_LOC_NAME = string.Empty;
            CHK_NO1 = string.Empty;
            SEQ = string.Empty;
            USE_QTY = string.Empty;
            MEMO = string.Empty;

            G34_MAX_APPQTY = string.Empty;// 單位請領基準量
            EST_APPQTY = string.Empty;// 下月預估申請量

            ORI_CHK_QTY = string.Empty;
            CHK_YM = string.Empty;
            CHK_WH_KIND = string.Empty;
            CHK_WH_GRADE = string.Empty;
            CHK_WH_NO = string.Empty;
            WH_NAME = string.Empty;
            USE_QTY_AF_CHK = string.Empty;

            IsPass = true;

        } // 
    } // ec
} // en