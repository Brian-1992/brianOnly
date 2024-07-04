using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class MM_PR_D : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }

        public string MAT_CLASS { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string M_DISCPERC { get; set; }
        public int INV_QTY { get; set; }
        public string BASE_UNIT { get; set; }    
        public string E_CODATE { get; set; }
        public string PR_NO { get; set; }
        public string AGEN_FAX { get; set; }
        public string AGEN_NAME { get; set; }
        public string AGEN_NO { get; set; }
        public int BW_SQTY { get; set; }
        public int DELI_QTY { get; set; }
        public string DISC { get; set; }
        public string IS_FAX { get; set; }
        public string IS_EMAIL { get; set; }
        public string M_CONTID { get; set; }
        public string M_CONTPRICE { get; set; }
        public string M_PURUN { get; set; }
        public string PR_PRICE { get; set; } 
        public string E_ITEMARMYNO { get; set; }
        public int PR_QTY { get; set; }
        public int REQ_QTY_T { get; set; }
        public int UNIT_SWAP { get; set; }
        public DateTime CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string REC_STATUS { get; set; }
        public string TOTAL_PRICE { get; set; }
        public string UPRICE { get; set; }
        public string TOT { get; set; }
        public int MIN_ORDQTY { get; set; }
        public int SRC_PR_QTY { get; set; }
        public int DIFF_PR_QTY { get; set; }

        public string MEMO { get; set; }
        public string DISC_CPRICE { get; set; }
        public string DISC_UPRICE { get; set; }


        public string SAFE_QTY { get; set; }
        public string OPER_QTY { get; set; }
        public string SHIP_QTY { get; set; }
        public string DISCOUNT_QTY { get; set; }
        public string CHECK_RESULT { get; set; }

        public string ORDERKINID { get; set; }
        public string CASENO { get; set; }
        public string E_SOURCECODE { get; set; }


        public int? Seq { get; set; }
        public string CHINNAME { get; set; }
        public string CHARTNO { get; set; }
    }
}
