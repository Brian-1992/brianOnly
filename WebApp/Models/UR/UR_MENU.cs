using System;

namespace WebApp.Models
{
    public class UR_MENU
    {
        public string FG { get; set; }
        public string PG { get; set; }
        public string SG { get; set; }
        public string FT { get; set; }
        public decimal FS { get; set; }
        public string F0 { get; set; }
        public string F1 { get; set; }
        public string FA0 { get; set; } //相對位置 (用FU+FUP取代)
        public string FU { get; set; } //File Url
        public string FUP { get; set; } //File Url Params
        public string FD { get; set; } //File Desc
        public decimal FL { get; set; } //上線
        public string ATTACH_NAME { get; set; }
        public string ATTACH_URL { get; set; }
        public string MFG { get; set; }
        public Nullable<decimal> MFS { get; set; }

        //fields for treestore 吉威
        public string id { get; set; }
        public string url { get; set; }
        public string text { get; set; }
        public string iconCls { get; set; }
        public bool expanded { get; set; }
        public bool leaf { get; set; }
    }
}