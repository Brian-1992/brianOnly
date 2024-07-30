using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class WHTRNS
    {
        public string WH_NO { get; set; }
        public string TR_DATE { get; set; }
        public string TR_SNO { get; set; }
        public string MMCODE { get; set; }
        public string TR_INV_QTY { get; set; }
        public string TR_ONWAY_QTY { get; set; }
        public string TR_DOCNO { get; set; }
        public string TR_DOCSEQ { get; set; }
        public string TR_FLOWID { get; set; }
        public string TR_DOCTYPE { get; set; }
        public string TR_IO { get; set; }
        public string TR_MCODE { get; set; }
        public string BF_TR_INVQTY { get; set; }
        public string AF_TR_INVQTY { get; set; }
        public string TR_MCODE_NAME { get; set; }
        public string LOTNO_EXPDATE_QTY { get; set; }
    }
}