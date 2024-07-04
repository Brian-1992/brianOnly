using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MM_PO_D : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string MMCODE { get; set; }
        public string PR_DEPT { get; set; }
        public string PO_QTY { get; set; }
        public string PO_PRICE { get; set; }
        public string M_PURUN { get; set; }
        public string M_AGENLAB { get; set; }
        public string PO_AMT { get; set; }
        public string SUM_PO_AMT { get; set; }
        public string M_DISCPERC { get; set; }
        public string DELI_QTY { get; set; }
        public string BW_QTY { get; set; }
        public string DELI_STATUS { get; set; }
        public string MEMO { get; set; }
        public string PR_NO { get; set; }
        public string UNIT_SWAP { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string INVOICE { get; set; }
        public string INVOICE_DT { get; set; }
        public string CKIN_QTY { get; set; }
        public string CHK_USER { get; set; }
        public string CHK_DT { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string E_MANUFACT { get; set; }
        public string STATUS { get; set; }

        public string CONTRACNO { get; set; }
        public string E_ORDERUNIT { get; set; }
        public string E_SOURCECODE { get; set; }
        public string SUM_PO_PRICE { get; set; }
        public string SELF_CONT_BDATE { get; set; }
        public string SELF_CONT_EDATE { get; set; }
        public string SELF_CONTRACT_NO { get; set; }
        public string BATCH_DELI_DATE { get; set; }
        public string ITEM_NO { get; set; }

        public string BASE_UNIT { get; set; }
        public string UNITRATE { get; set; }
        public string M_NHIKEY { get; set; }
        public string ORDERKIND { get; set; }

        public string UPRICE { get; set; }
        public string DISC_CPRICE { get; set; }
        public string DISC_UPRICE { get; set; }

        public string CASENO { get; set; }
        public string E_CODATE { get; set; }
        public string MEMO_D { get; set; }
        public string SMEMO { get; set; }
        public string AGEN_NO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string AGEN_TEL { get; set; }
        public string AGEN_FAX { get; set; }
        public string EMAIL { get; set; }
        public string MAT_CLASS { get; set; }
        public string PO_TIME { get; set; }
        public string STORE_LOC { get; set; }
        //BD0014用MI_MAST.UNITRATE
        public string MAST_UNITRATE { get; set; }

        public string CHINNAME { get; set; }
        public string CHARTNO { get; set; }
        public string PARTIALDL_DT { get; set; }
    }
}