using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppVen.Models
{
    public class MI_MAST : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }

        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string MAT_CLASS { get; set; }
        public string BASE_UNIT { get; set; }
        public string AUTO_APLID { get; set; }
        public string M_STOREID { get; set; }
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
        public DateTime E_CODATE { get; set; }
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
        public string WEXP_ID { get; set; }
        public string WLOC_ID { get; set; }

        // =================== 湘倫 ======================
        public string MMNAME { get; set; }      // 自訂 MMCODE + MMNAME_C
        // ===============================================

        // =================== 易展 ======================
        public string AVG_PRICE { get; set; }  //庫存平均單價
        public string INV_QTY { get; set; }    //庫存數量
        public string AVG_APLQTY { get; set; } //平均申請數量
        public string A_INV_QTY { get; set; } //庫存數量
        public string B_INV_QTY { get; set; } //庫存數量
        // ===============================================
    }
}