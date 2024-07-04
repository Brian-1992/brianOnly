using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_MDFM : JCLib.Mvc.BaseModel
    {
        public string MDFM { get; set; }
        public string MD_NAME { get; set; }
        public string MMCODE { get; set; }
        public string MDFM_QTY { get; set; }
        public string MDFM_UNIT { get; set; }
        public string USE_QTY { get; set; }
        public string PRESERVE_DAYS { get; set; }
        public string OPERATION { get; set; }
        public string ELEMENTS { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_ID { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_ID { get; set; }
        public string UPDATE_IP { get; set; }
        //==========
        public string DOCNO { get; set; }
        public string DOCTYPE { get; set; }
        public string FLOWID { get; set; }
        public string APPID { get; set; }
        public string APPDEPT { get; set; }
        public string APPTIME { get; set; }
        public string USEID { get; set; }
        public string USEDEPT { get; set; }
        public string FRWH { get; set; }
        public string TOWH { get; set; }
        public string LIST_ID { get; set; }
        public string APPLY_KIND { get; set; }
        public string APPLY_NOTE { get; set; }
        public string MAT_CLASS { get; set; }
        //public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        //public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        //public string UPDATE_IP { get; set; }
        public string FLOWID_N { get; set; }
        public string MAT_CLASS_N { get; set; }
        public string FRWH_N { get; set; }
        public string TOWH_N { get; set; }
        public string APPLY_KIND_N { get; set; }
        public string APPTIME_T { get; set; }
        public string MR1 { get; set; }
        public string MR2 { get; set; }
        public string MR3 { get; set; }
        public string MR4 { get; set; }

        // =================== 湘倫 ======================
        public string APP_NAME { get; set; }      // 自訂 APPID || ' ' || USER_NAME(APPID)
        public string APPDEPT_NAME { get; set; }  // 自訂 APPDEPT || ' ' || INID_NAME(APPDEPT)
        public string USEDEPT_NAME { get; set; }  // 自訂 USEDEPT || ' ' || INID_NAME(USEDEPT)
        public string FRWH_NAME { get; set; }     // 自訂 FRWH || ' ' || WH_NAME(FRWH)
        public string TOWH_NAME { get; set; }     // 自訂 TOWH || ' ' || WH_NAME(TOWH)
        // ===============================================

        //====================紹朋========================
        public string APPTIME_TWN { get; set; }
        // ===============================================

        // =================== 家瑋 ======================
        public string CREATE_USER_NAME { get; set; }
        // ===============================================
        // =================== 重凱 ======================
        public int CHECKSEQ { get; set; }
        public string GENWAY { get; set; }
        public string ACKQTY { get; set; }
        public string APVQTY { get; set; }
        //public string MMCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string AGEN_NAMEE { get; set; }
        public string WH_NAME { get; set; }
        public string DATA_YM { get; set; }
        public string WH_NO { get; set; }
        public string E_SUPSTATUS { get; set; }
        public string INV_QTY { get; set; }
        public string MINV_QTY { get; set; }
        public string APL_INQTY { get; set; }
        public string APL_OUTQTY { get; set; }
        public string CNT_QTY { get; set; }
        public string ADJ_QTY { get; set; }
        public string BAL_QTY { get; set; }
        public string E_ITEMARMYNO { get; set; }
        public string E_COMPUNIT { get; set; }
        public string E_MANUFACT { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTPRICE { get; set; }
        public float TOTAL { get; set; }
        public string SEQ { get; set; }
        // ===============================================
    }
}
