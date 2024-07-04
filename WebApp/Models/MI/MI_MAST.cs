using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MI_MAST : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }

        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string MAT_CLASS { get; set; }
        public string MAT_CLASS_SUB { get; set; }
        public string BASE_UNIT { get; set; }
        public string AUTO_APLID { get; set; }
        public string M_STOREID { get; set; }
        public string MONEYCHANGE { get; set; }
        public string M_CONTID { get; set; }
        public string M_IDKEY { get; set; }
        public string M_INVKEY { get; set; }
        public string M_NHIKEY { get; set; }
        public string M_GOVKEY { get; set; }
        public string M_VOLL { get; set; }
        public string M_VOLW { get; set; }
        public string M_VOLH { get; set; }
        public string M_VOLC { get; set; }
        public string M_SWAP { get; set; }
        public string M_MATID { get; set; }
        public string M_SUPPLYID { get; set; }
        public string M_CONSUMID { get; set; }
        public string M_PAYKIND { get; set; }
        public string M_PAYID { get; set; }
        public string M_TRNID { get; set; }
        public string M_APPLYID { get; set; }
        public string M_PHCTNCO { get; set; }
        public string M_ENVDT { get; set; }
        public string M_DISTUN { get; set; }
        public string M_AGENNO { get; set; }
        public string M_AGENLAB { get; set; }
        public string M_PURUN { get; set; }
        public string M_CONTPRICE { get; set; }
        public string M_DISCPERC { get; set; }
        public string E_SUPSTATUS { get; set; }
        public string E_MANUFACT { get; set; }
        public string E_IFPUBLIC { get; set; }
        public string E_STOCKTYPE { get; set; }
        public string E_SPECNUNIT { get; set; }
        public string E_COMPUNIT { get; set; }
        public string E_YRARMYNO { get; set; }
        public string E_ITEMARMYNO { get; set; }
        public string E_GPARMYNO { get; set; }
        public string E_CLFARMYNO { get; set; }
        public DateTime? E_CODATE { get; set; }
        public string E_CODATE_T { get; set; }
        public string E_PRESCRIPTYPE { get; set; }
        public string E_DRUGCLASS { get; set; }
        public string E_DRUGCLASSIFY { get; set; }
        public string E_DRUGFORM { get; set; }
        public string E_COMITMEMO { get; set; }
        public string E_COMITCODE { get; set; }
        public string E_INVFLAG { get; set; }
        public string E_PURTYPE { get; set; }
        public string E_SOURCECODE { get; set; }
        public string E_DRUGAPLTYPE { get; set; }
        public string E_ARMYORDCODE { get; set; }
        public string E_PARCODE { get; set; }
        public string E_PARORDCODE { get; set; }
        public string E_SONTRANSQTY { get; set; }
        public string CANCEL_ID { get; set; }
        public string E_RESTRICTCODE { get; set; }
        public string E_ORDERDCFLAG { get; set; }
        public string E_HIGHPRICEFLAG { get; set; }
        public string E_RETURNDRUGFLAG { get; set; }
        public string E_RESEARCHDRUGFLAG { get; set; }
        public string E_VACCINE { get; set; }
        public string E_TAKEKIND { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string E_FREQNOO { get; set; } //門診給藥頻率
        public string E_FREQNOI { get; set; } //住院給藥頻率
        public string CONTRACNO { get; set; } //合約碼
        public string UPRICE { get; set; } //最小單價
        public string DISC_CPRICE { get; set; } //優惠合約單價
        public string DISC_UPRICE { get; set; } //優惠最小單價(計量單位單價)
        public string WEXP_ID { get; set; }
        public string WLOC_ID { get; set; }
        public string EASYNAME { get; set; }
        public string CANCEL_NOTE { get; set; }
        public string NHI_PRICE { get; set; }
        public string HOSP_PRICE { get; set; }

        // =================== 湘倫 ======================
        public string MMNAME { get; set; }      // 自訂 MMCODE + MMNAME_C
        public string MIL_PRICE { get; set; }
        public string TOTAL_PRICE { get; set; }
        public string ROW_NUM { get; set; }
        public string SAFE_QTY { get; set; }
        public string OPER_QTY { get; set; }
        public string TO_INV_QTY { get; set; }
        public string FR_INV_QTY { get; set; }
        public string APP_QTY_NOT_APPROVED { get; set; }
        public string SUPPLY_WHNO { get; set; }
        // ===============================================

        // =================== 易展 ======================
        public string AVG_PRICE { get; set; }  //庫存平均單價
        public string INV_QTY { get; set; }    //庫存數量
        public string AVG_APLQTY { get; set; } //平均申請數量
        public string A_INV_QTY { get; set; } //庫存數量
        public string B_INV_QTY { get; set; } //庫存數量
        public string AGEN_NAMEC { get; set; } //廠商中文名稱
        public string AGEN_NAMEE { get; set; } //廠商英文名稱
        public string EXCH_RATIO { get; set; } //轉換率
        public string M_INV_QTY { get; set; } //庫存數量
        public string S_INV_QTY { get; set; } //庫存數量
        public string HIGH_QTY { get; set; } //基準量
        public string TOT_APVQTY { get; set; } //累計核撥量
        public string TOT_DISTUN { get; set; }
        // ===============================================

        // =================== 姵蓁 ======================
        public string E_ORDERUNIT { get; set; } //醫囑單位
        public string E_PATHNO { get; set; } //院內給藥途徑(使用途徑)
        public string INSUORDERCODE { get; set; } //健保碼
        public string ORDERHOSPNAME { get; set; } //別名(院內名稱)
        public string ORDEREASYNAME { get; set; } //簡稱
        public string SCIENTIFICNAME { get; set; } //成份名稱
        public string CURECONSISTENCY { get; set; } //合理治療濃度
        public string PEAR { get; set; } //合理PEAK
        public string TROUGH { get; set; } //合理 Trough
        public string DANGER { get; set; } //危急值
        public string TDMFLAG { get; set; } //TDM藥品(Y/N)
        public string TDMMEMO1 { get; set; } //備註1
        public string TDMMEMO2 { get; set; } //備註2
        public string TDMMEMO3 { get; set; } //備註3
        public string UDSERVICEFLAG { get; set; } //使用自動調配機(Y/N)
        public string UDPOWDERFLAG { get; set; } //UD磨粉(Y/N)
        public string AIRDELIVERY { get; set; } //可氣送(Y/N)
        public string PFILE_ID { get; set; } //附件上傳ID


        public string MAT_CLSNAME { get; set; } //品項別
        public string UNITRATE { get; set; } //出貨單位

        // ===============================================

        // =================== 家瑋 ======================
        public string STORE_LOC { get; set; }
        // ===============================================

        public string MIN_ORDQTY { get; set; }
        public string MAT_CLSID { get; set; }

        public string BEGINDATE { get; set; }
        public string ENDDATE { get; set; }
        public string E_STOCKTRANSQTYI { get; set; }
        public string E_SCIENTIFICNAME { get; set; }

        public string BEGINDATE_DATE { get; set; }
        public string ENDDATE_DATE { get; set; }

        public string SaveStatus { get; set; }
        public string UploadMsg { get; set; }

        public int Seq { get; set; }

        public string WHMM_VALID {get;set;}
        public string ISWILLING { get; set; }
        public string DISCOUNT_QTY { get; set; }
        public string DISC_COST_UPRICE { get; set; }

        public string SELF_BID_SOURCE { get; set; }
        public string SELF_CONTRACT_NO { get; set; }
        public string SELF_PUR_UPPER_LIMIT { get; set; }
        public string SELF_CONT_BDATE { get; set; }
        public string SELF_CONT_EDATE { get; set; }

        public string ISSMALL { get; set; }

        public string DRUGSNAME { get; set; }
        public string DRUGHIDE { get; set; }
        public string CANCEL_ID_DESC { get; set; }
        public string E_ORDERDCFLAG_DESC { get; set; }
        public string HEALTHOWNEXP { get; set; }
        public string ISSUESUPPLY { get; set; }
        public string BASE_UNIT_DESC { get; set; }
        public string M_PURUN_DESC { get; set; }
        public string TRUTRATE { get; set; }
        public string MAT_CLASS_SUB_DESC { get; set; }
        public string E_RESTRICTCODE_DESC { get; set; }
        public string WARBAK { get; set; }
        public string WARBAK_DESC { get; set; }
        public string ONECOST { get; set; }
        public string ONECOST_DESC { get; set; }
        public string HEALTHPAY { get; set; }
        public string HEALTHPAY_DESC { get; set; }
        public string COSTKIND { get; set; }
        public string COSTKIND_DESC { get; set; }
        public string WASTKIND { get; set; }
        public string WASTKIND_DESC { get; set; }
        public string SPXFEE { get; set; }
        public string SPXFEE_DESC { get; set; }
        public string ORDERKIND { get; set; }
        public string ORDERKIND_DESC { get; set; }
        public string DRUGKIND { get; set; }
        public string DRUGKIND_DESC { get; set; }
        public string SPDRUG { get; set; }
        public string SPDRUG_DESC { get; set; }
        public string FASTDRUG { get; set; }
        public string FASTDRUG_DESC { get; set; }
        public string MIMASTHIS_SEQ { get; set; }
        public string MIMASTHIS_SEQ_NEW { get; set; }
        public string CASENO { get; set; }
        public string M_CONTID_DESC { get; set; }
        public string CONTRACTAMT { get; set; }
        public string CONTRACTSUM { get; set; }
        public string TOUCHCASE { get; set; }
        public string TOUCHCASE_DESC { get; set; }
        public DateTime BEGINDATE_14 { get; set; }
        public string BEGINDATE_14_T { get; set; }
        public DateTime EFFSTARTDATE { get; set; }
        public string EFFSTARTDATE_T { get; set; }
        public DateTime EFFENDDATE { get; set; }
        public string EFFENDDATE_T { get; set; }
        public DateTime ISSPRICEDATE { get; set; }
        public string ISSPRICEDATE_T { get; set; }
        public string CASEDOCT { get; set; }
        public string BARCODE { get; set; }
        public string MMCODE_BARCODE { get; set; }
        public string SPMMCODE { get; set; }
        public string COMMON { get; set; }
        public string APPQTY_TIMES { get; set; }
        public string UI_CHANAME { get; set; }
        public string ISIV { get; set; }

        // 前端計算用
        public string JBID_RCRATE { get; set; }
        public string CHECK_RESULT { get; set; }

        public string UNI_NO { get; set; }
    }
}