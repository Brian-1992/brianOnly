using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MI_MAST_HISTORY : JCLib.Mvc.BaseModel
    {
        public string MIMASTHIS_SEQ { get; set; }
        public string MMCODE { get; set; }
        public string EFFSTARTDATE { get; set; }
        public string EFFSTARTDATE_T { get; set; }
        public string EFFENDDATE { get; set; }
        public string EFFENDDATE_T { get; set; }
        public string CANCEL_ID { get; set; }
        public string E_ORDERDCFLAG { get; set; }
        public string M_STOREID { get; set; }
        public string M_STOREID_DESC { get; set; }
        public string M_NHIKEY { get; set; }
        public string HEALTHOWNEXP { get; set; }
        public string DRUGSNAME { get; set; }
        public string MMNAME_E { get; set; }
        public string MMNAME_C { get; set; }
        public string M_PHCTNCO { get; set; }
        public string M_ENVDT { get; set; }
        public string ISSUESUPPLY { get; set; }
        public string E_MANUFACT { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_PURUN { get; set; }
        public string TRUTRATE { get; set; }
        public string UNITRATE { get; set; }
        public string MAT_CLASS_SUB { get; set; }
        public string E_RESTRICTCODE { get; set; }
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
        public string M_AGENNO { get; set; }
        public string EASYNAME { get; set; }
        public string M_AGENLAB { get; set; }
        public string CASENO { get; set; }
        public string CASEDOCT { get; set; }
        public string E_SOURCECODE { get; set; }
        public string E_SOURCECODE_DESC { get; set; }
        public string M_CONTID { get; set; }
        public string M_CONTID_DESC { get; set; }
        public string E_ITEMARMYNO { get; set; }
        public string NHI_PRICE { get; set; }
        public string DISC_CPRICE { get; set; }
        public string M_CONTPRICE { get; set; }
        public string E_CODATE { get; set; }
        public string E_CODATE_T { get; set; }
        public string CONTRACTAMT { get; set; }
        public string CONTRACTSUM { get; set; }
        public string TOUCHCASE { get; set; }
        public string TOUCHCASE_DESC { get; set; }
        public string BEGINDATE_14 { get; set; }
        public string BEGINDATE_14_T { get; set; }
        public string ISSPRICEDATE { get; set; }
        public string ISSPRICEDATE_T { get; set; }
        public string CHECK_RESULT { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_TIME_T { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string COMMON { get; set; }
        public string COMMON_T { get; set; }
        public string SPMMCODE { get; set; }
        public string DISCOUNT_QTY { get; set; }
        public string APPQTY_TIMES { get; set; }
        public string ISIV { get; set; }
        public string DISC_COST_UPRICE { get; set; }

        // 歷史紀錄用
        public string MONEYCHANGE { get; set; }
    }
}