using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_DOCM : JCLib.Mvc.BaseModel
    {
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
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string JCN { get; set; }
        public string UPDATE_IP { get; set; }
        public string STKTRANSKIND { get; set; }
        public string AGEN_NO { get; set; }
        // =================== 易展 ======================
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
        public string DOCTYPE_N { get; set; }
        public string SUM_EX { get; set; }
        public string SUM_IN { get; set; }
        public string STKTRANSKIND_N { get; set; }
        public string AGEN_NO_N { get; set; }
        public string WEXP_YN { get; set; }
        public string M_STOREID { get; set; }
        public string M_STOREID_T { get; set; }
        public string POSTID { get; set; }
        public string EXT { get; set; }
        public string M_AGENNO { get; set; }
        public string POST_TIME { get; set; }
        public string APPLY_DATE { get; set; }
        // ===============================================

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

        public string APPCNT { get; set; }
        public string APPQTY_SUM { get; set; }

        public string RETURN_NOTE { get; set; }

        public string ISCRNOTE { get; set; }
        public string ISCR { get; set; }
        // ===============================================
        // =================== 重凱 ======================
        public int CHECKSEQ { get; set; }
        public string GENWAY { get; set; }
        public string ACKQTY { get; set; }
        public string APVQTY { get; set; }
        public string ACKTIME { get; set; }
        public string APVTIME { get; set; }
        public string APVID { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
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
        public string TOTAL { get; set; }
        public string SEQ { get; set; }
        public string AMT { get; set; }
        public string APPQTY { get; set; }
        public string INC_QTY { get; set; }
        public string INC_PRICE { get; set; }
        public string FRWH_D { get; set; }
        //public string STKTRANSKIND { get; set; }
        public string APLYITEM_NOTE { get; set; }
        public string GTAPL_RESON { get; set; }
        public string FRWHID { get; set; }
        public string CONFIRMSWITCH { get; set; }
        public string CONTRACNO { get; set; }
        // ===============================================
        public string AVG_PRICE { get; set; }
        // ===============================================
        public string NRCODE { get; set; }
        public string BEDNO { get; set; }
        public string MEDNO { get; set; }
        public string CHINNAME { get; set; }
        public string ORDERDATE { get; set; }

        public string E_SCIENTIFICNAME { get; set; }
        public string AGEN_NAME { get; set; }


        public string SENDAPVID { get; set; }
        public string SENDAPV_NAME { get; set; }
        public string SENDAPVTIME { get; set; }
        public string SENDAPVTIME_T { get; set; }

        public string WH_KIND { get; set; }
        //=============秀甄=============
        public string TOWH_NO { get; set; }
        public string SRCDOCNO { get; set; }
        public string ISCONTID3 { get; set; }
        public string BALANCE { get; set; }
        public string UP { get; set; }
        public string SRCDOCYN { get; set; }
        public string WH_GRADE { get; set; }
        public string ITEM_STRING { get; set; }
        public string isDis { get; set; }
        public string RDOCNO { get; set; }
        public string HOSP_INFO { get; set; }
        //=============睬絜=============
        public string ISARMY { get; set; }
        public string ISARMY_N { get; set; }
        public string APPUNA { get; set; }
        public string APP_INID { get; set; }
        public string APP_INID_NAME { get; set; }
        public string M_CONTID { get; set; }
        public string M_CONTID_T { get; set; }
        public string DISC_CPRICE { get; set; }

        public string M_STOREID_N { get; set; }
    }
}
