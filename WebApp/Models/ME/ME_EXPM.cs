using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ME_EXPM : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string EXP_DATE { get; set; }
        public string WARNYM { get; set; }
        public string LOT_NO { get; set; }
        public string MEMO { get; set; }
        public string CLOSEFLAG { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string EXP_QTY { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTPRICE { get; set; }
        public string M_AGENNO { get; set; }
        public string MMCODE2 { get; set; }
        public string MMNAME_C2 { get; set; }
        public string MMNAME_E2 { get; set; }
        public string BASE_UNIT2 { get; set; }
        public string EXP_QTY2 { get; set; }
        public string EXP_DATE2 { get; set; }
        public string EXP_DATE_N { get; set; }
        public string EXP_DATE_T { get; set; }
        public string CLOSEFLAG_N { get; set; }
        public string FLOWID { get; set; }
        public string FLOWID_N { get; set; }
        public string DOCNO_M { get; set; }
        public string DOCNO_E { get; set; }
        public string RDOCNO { get; set; }
        public string M_DISCPERC { get; set; }
    }
}