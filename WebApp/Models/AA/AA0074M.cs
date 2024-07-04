using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class AA0074M : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string INV_QTY { get; set; }
        public string EX_INV_QTY { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string MMNAME_C { get; set; }        //中文品名
        public string MMNAME_E { get; set; }        //英文品名
        public string MMNAME { get; set; }
        public string MAT_CLASS { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTID { get; set; }
        public string MIL { get; set; }
        public string DATA_YM { get; set; }
        public string M_AGENNO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string INV_QTY_INCR { get; set; }
        public string INV_QTY_DECR { get; set; }
        public string APL_OUTQTY { get; set; }
        public string APL_INQTY { get; set; }
        
    }
}