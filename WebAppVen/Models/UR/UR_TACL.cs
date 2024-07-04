using System;

namespace WebAppVen.Models
{
    public class UR_TACL
    {
        public string FG { get; set; }
        public string RLNO { get; set; }
        public Nullable<decimal> G { get; set; }
        public Nullable<decimal> S { get; set; }
        public Nullable<decimal> V { get; set; }
        public Nullable<decimal> U { get; set; }
        public Nullable<decimal> P { get; set; }
        public Nullable<decimal> R { get; set; }
        public Nullable<System.DateTime> TACL_CREATE_DATE { get; set; }
        public string TACL_CREATE_BY { get; set; }
        public Nullable<System.DateTime> TACL_MODIFY_DATE { get; set; }
        public string TACL_MODIFY_BY { get; set; }
    }
}