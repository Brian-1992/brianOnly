using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ME_DOCE : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public int SEQ { get; set; }
        public DateTime EXPDATE { get; set; }
        public string MMCODE { get; set; }
        public int APVQTY { get; set; }
        public string APVID { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string LOT_NO { get; set; }
        public string MEMO { get; set; }
        public string PROC_ID { get; set; }

        public string WARNYM { get; set; }
        public string UPDATE_USER_NAME { get; set; }
        public string ITEM_STRING { get; set; }
        public DateTime ORI_EXPDATE { get; set; }

        // =================== 易展 ======================
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public float M_CONTPRICE { get; set; }
        public string M_AGENNO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string AGEN_NAMEE { get; set; }
        public string EXPDATET { get; set; }
        public string DOCNO_M { get; set; }
        public string DOCNO_E { get; set; }
        public string MMCODE1 { get; set; }
        public int EXP_QTY { get; set; }
        public float M_DISCPERC { get; set; }
        public string MMCODE_O { get; set; }
        public string LOT_NO_O { get; set; }
        public string EXPDATE_O { get; set; }
        public int EXP_QTY_O { get; set; }
        // ===============================================
    }
}