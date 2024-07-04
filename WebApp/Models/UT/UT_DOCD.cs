using System;

namespace WebApp.Models.UT
{
    public class UT_DOCD
    {
        public string DOCNO { get; set; }
        public string DOCTYPE { get; set; }
        public Int16 SEQ { get; set; }
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public DateTime EXP_DATE { get; set; }
        public string LOT_NO { get; set; }
        public float INV_QTY { get; set; }
        public string EXP_DATE_LIST { get; set; }
        public string LOT_NO_LIST { get; set; }
        public string APVQTY_LIST { get; set; }
        public string APVQTY { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string ISFOUND { get; set; }
    }
}