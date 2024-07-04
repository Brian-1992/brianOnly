using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MM_PO_M : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string AGEN_NO { get; set; }
        public string PO_TIME { get; set; }
        public string M_CONTID { get; set; }
        public string PO_STATUS { get; set; }
        public string MEMO { get; set; }
        public string ISCONFIRM { get; set; }
        public string ISBACK { get; set; }
        public string PHONE { get; set; }
        public string SMEMO { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string PO_TIME_N { get; set; }
        public string CNT { get; set; }

        public string PO_DATE { get; set; }
        public string WH_NO { get; set; }
        public string CONTRACNO { get; set; }
        public string REPLY_DT { get; set; }
        public string MAT_CLASS { get; set; }
        public string EMAIL { get; set; }
        public string REPLY_DELI { get; set; }
        public string REPLY_CNT { get; set; }

        public string AGEN_NAMEC { get; set; }

        public string ISCR { get; set; }
        public string MMCODE_OVER_150K { get; set; }
        public string EASYNAME { get; set; }
        public string MAT_CLASS_NAME { get; set; }
    }
}