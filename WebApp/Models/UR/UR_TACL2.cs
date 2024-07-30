using System;

namespace WebApp.Models
{
    public class UR_TACL2 : JCLib.Mvc.BaseModel
    {
        public string FG { get; set; }
        public string TUSER { get; set; }
        public string UNA { get; set; }
        public Nullable<decimal> G { get; set; }
        public Nullable<decimal> S { get; set; }
        public Nullable<decimal> V { get; set; }
        public Nullable<decimal> U { get; set; }
        public Nullable<decimal> P { get; set; }
        public Nullable<decimal> R { get; set; }
        public Nullable<System.DateTime> TACL_CREATE_DATE { get; set; }
        public string TACL_CREATE_BY { get; set; }
        public string TACL_CREATE_UNA { get; set; }
        public Nullable<System.DateTime> TACL_MODIFY_DATE { get; set; }
        public string TACL_MODIFY_BY { get; set; }
        public string ADUSER { get; set; }
        // ===============================
        public string RLNO { get; set; }
        public string RLNA { get; set; }
        public string RLDESC { get; set; }
        public string INID { get; set; }
        public string INID_NAME { get; set; }
        public string F1 { get; set; }
    }
}