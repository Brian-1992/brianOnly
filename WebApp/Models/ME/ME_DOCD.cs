using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_DOCD : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string SEQ { get; set; }
        public string MMCODE { get; set; }
        public string APPQTY { get; set; }
        public string APVQTY { get; set; }
        public string APVTIME { get; set; }
        public string APVID { get; set; }
        public string ACKQTY { get; set; }
        public string ACKID { get; set; }
        public string ACKTIME { get; set; }
        public string STAT { get; set; }
        public string RSEQ { get; set; }
        public string EXPT_DISTQTY { get; set; }
        public string DIS_USER { get; set; }
        public string DIS_TIME { get; set; }
        public string BW_MQTY { get; set; }
        public string BW_SQTY { get; set; }
        public string PICK_QTY { get; set; }
        public string PICK_USER { get; set; }
        public string PICK_TIME { get; set; }
        public string ONWAY_QTY { get; set; }
        public string APL_CONTIME { get; set; }
        public string APLYITEM_NOTE { get; set; }
        public string AMT { get; set; }
        public string UP { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string RV_MQTY { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTPRICE { get; set; }
        public string AVG_PRICE { get; set; }
        public string INV_QTY { get; set; }
        public string AVG_APLQTY { get; set; }
        public string ONWAYQTY { get; set; }
        public string AFTERQTY { get; set; }
        public string SAFE_QTY { get; set; }
        public string ACKTIME_T { get; set; }
        public string STAT_N { get; set; }
        public string A_INV_QTY { get; set; }
        public string B_INV_QTY { get; set; }
        public string M_DISCPERC { get; set; }
        public string TOT_PRICE { get; set; }
        public string BALANCE { get; set; }
        public string TOT_APVQTY { get; set; }
        public string TOT_BWQTY { get; set; }
        public string ITEM_STRING { get; set; }
        public string HIGH_QTY { get; set; }
        public string SGN { get; set; }
        public string GTAPL_RESON { get; set; }
        public string STKTRANSKIND { get; set; }
        public string APVQTY_C { get; set; }
        public string TRNAB_QTY { get; set; }
        public string TRNAB_RESON { get; set; }
        public string TRNAB_RESON_TEXT { get; set; }
        // =================== 易展 ======================
        public string LOT_NO { get; set; }
        public string LOT_NO_N { get; set; }
        public string EXPDATE { get; set; }
        public string EXPDATE_T { get; set; }
        public string DIS_TIME_T { get; set; }
        public string TOT_DISTUN { get; set; }
        public string GTAPL_RESON_N { get; set; }
        public string PFILE_ID { get; set; }
        public string SUM_EX { get; set; }
        public string SUM_IN { get; set; }
        public string MIL_PRICE { get; set; }
        public string CHECK_RESULT { get; set; }
        public string IMPORT_RESULT { get; set; }
        public string FRWH { get; set; }
        public string TOWH { get; set; }
        public string CTDMDCCODE { get; set; }
        public string STATUS1 { get; set; }
        public string STATUS2 { get; set; }
        public string STATUS3 { get; set; }
        public string STATUS4 { get; set; }
        public string RDOCNO { get; set; }
        public string DRUGMEMO { get; set; }
        public string MAT_CLASS { get; set; }
        // ===============================================
        // =================== 湘倫 ======================
        public string INV_QTY_FR { get; set; }
        public string INV_QTY_TO { get; set; }
        public string SUGGEST_QTY { get; set; }
        public string FRWH_D { get; set; }
        public string S_INV_QTY { get; set; }   // 上級庫庫存量
        public string OPER_QTY { get; set; }   // 基準量
        public string PACK_QTY { get; set; }   // 包裝劑量
        public string PACK_UNIT { get; set; }   // 包裝單位
        public string E_ORDERDCFLAG { get; set; }   // 藥品停用碼
        public string APL_QTY { get; set; }
        public string ARMY_QTY { get; set; }
        public string ARMY_TOTAL_QTY { get; set; }
        public string STORE_LOC { get; set; }
        public string APP_QTY_NOT_APPROVED { get; set; }
        public string TRANSKIND { get; set; }
        public string TRANSKIND_NAME { get; set; }
        public string POSTID { get; set; }
        public string POSTIDC { get; set; }
        public string LAST_X_MONTH_ITEM { get; set; }
        public string LAST_X_MONTH_APPQTY { get; set; }
        public string MMCODE_NAME { get; set; }
        public string DOCNO_BARCODE { get; set; }
        // ===============================================

        public string BARCODE { get; set; }
        public string TRATIO { get; set; }

        // =================== 家瑋 ======================
        public string AMOUNT { get; set; }
        public string WH_NO { get; set; }
        public string TOWH_STORE_QTY { get; set; }

        #region 2020-05-21 修改AA0015新增
        public string TOWH_NAME { get; set; }
        public string FRWH_NAME { get; set; }
        #endregion

        #region 2020-07-08 修改AB0012新增
        public string DISC_CPRICE { get; set; }
        #endregion

        #region 2020-08-25 修改AB0010新增 最低庫存量
        public string LOW_QTY { get; set; }
        #endregion

        #region 2020-09-30 修改AA0015新增 強迫點收數量
        public string ACKSYSQTY { get; set; }
        #endregion

        #region 2021-02-20 配合MI_WHMM新增是否可申領欄位
        public string WHMM_VALID { get; set; }
        #endregion

        #region 2022-11-18 
        public string IS_APPQTY_VALID { get; set; }
        #endregion


        // ===============================================

        public string WEXP_ID { get; set; }
        public string WEXP_ID_DESC { get; set; }

        // =================== 惠軒 ======================
        public string WH_NAME { get; set; }
        public string APPTIME { get; set; }
        public string APPDEPT { get; set; }
        public string APPDEPTNAME { get; set; }        
            

        // =================== 俊維 ======================
        public string DIS_DATEYM { get; set; }

        public string FLOWID { get; set; }


        // =================== 家瑋 =======================
        public string M_STOREID { get; set; }
        public string M_STOREID_T { get; set; }
        public string ACKQTYT { get; set; }


        public string DUEQTY { get; set; }

        public string UPRICE { get; set; }
        public string MIL_PRICE_TEMP { get; set; }

        public string HAS_POST { get; set; }

        public string PACK_TIMES { get; set; }
        public string ISSPLIT { get; set; }
        public string M_AGENNO { get; set; }
        public string CREATE_USER_NAME { get; set; }
        //=============秀甄=============
        public string TOWH_NO { get; set; }

        #region 2022-05-11 秀甄修改AB0010新增 建議申請量_90.安全量_90.基準量_90 
        public string APLY_QTY_90 { get; set; }
        public string SAFE_QTY_90 { get; set; }
        public string HIGH_QTY_90 { get; set; }
        #endregion

        public string SRCDOCNO { get; set; }
        public string DISC_UPRICE { get; set; }
        public string APP_AMT { get; set; }
        public string ISTRANSPR { get; set; }
        public string SHORT_REASON { get; set; }
        public string CAN_DIST_QTY { get; set; }
        //=============睬絜=============
        public string CASENO { get; set; }
        public string E_CODATE { get; set; }
        public string M_CONTID { get; set; }
        public string CHINNAME { get; set; }
        public string CHARTNO { get; set; }

        public string SUP_PATNAME { get; set; }
        public string SUP_MEDNO { get; set; }
        public string M_CONTID_T { get; set; }
        public string APPQTY_TIMES { get; set; } //申請倍數
        public string UNITRATE { get; set; } //出貨單位

        public string LAST_QTY { get; set; } //繳回後剩餘存量
        public string PR_QTY { get; set; }
        public string REST_QTY { get; set; }
        public string CHINNAME_OLD { get; set; }
        public string CHARTNO_OLD { get; set; }
        public string NORMAL_QTY { get; set; }

       public string APVQTY_O { get; set; }
       public string PRQTY_O { get; set; }
    }
}
