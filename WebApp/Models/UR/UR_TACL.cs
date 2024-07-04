using System;

namespace WebApp.Models
{
    public class UR_TACL : JCLib.Mvc.BaseModel
    {
        public string FG { get; set; }
        public string RLNO { get; set; }
        public Int16? G { get; set; }
        public Int16? S { get; set; }
        public Int16? V { get; set; }
        public Int16? U { get; set; }
        public Int16? P { get; set; }
        public Int16? R { get; set; }
        public DateTime? TACL_CREATE_DATE { get; set; }
        public string TACL_CREATE_BY { get; set; }
        public DateTime? TACL_MODIFY_DATE { get; set; }
        public string TACL_MODIFY_BY { get; set; }
        public bool UPD_FLAG { get; set; }
    }
}