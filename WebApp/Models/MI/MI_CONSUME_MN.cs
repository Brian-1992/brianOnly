using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class MI_CONSUME_MN
    {
        public string DATA_YM { get; set; }
        public string MMCODE { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_NHIKEY { get; set; }
        public string E_ORDERDCFLAG { get; set; }
        public string INSYQTY1 { get; set; }
        public string HOSPQTY1 { get; set; }
        public string ALLQTY1 { get; set; }
        public string INSYQTY3 { get; set; }
        public string HOSPQTY3 { get; set; }
        public string ALLQTY3 { get; set; }
        public string INSU_PRICE { get; set; }
        public string HOSP_PRICE { get; set; }
        public string P_INV_QTY { get; set; }
        public string INV_QTY { get; set; }
        public string DISC_UPRICE { get; set; }
        public string AVG_PRICE { get; set; }
        public string PMN_AVGPRICE { get; set; }
        public string OTH_CONSUME { get; set; }

        public string INSYQTY2 { get; set; }
        public string HOSPQTY2 { get; set; }
        public string ALLQTY2 { get; set; }


        public string ALLQTY { get; set; } // 門急住總耗量

        public MI_CONSUME_MN() {
            DATA_YM = string.Empty;
            MMCODE = string.Empty;
            BASE_UNIT = string.Empty;
            M_NHIKEY = string.Empty;
            E_ORDERDCFLAG = string.Empty;
            INSYQTY1 = "0";
            HOSPQTY1 = "0";
            ALLQTY1 = "0";
            INSYQTY3 = "0";
            HOSPQTY3 = "0";
            ALLQTY3 = "0";
            INSU_PRICE = "0";
            HOSP_PRICE = "0";
            P_INV_QTY = "0";
            INV_QTY = "0";
            DISC_UPRICE = "0";
            AVG_PRICE = "0";
            PMN_AVGPRICE = "0";
            OTH_CONSUME = "0";
            ALLQTY = "0";

            INSYQTY2 = "0";
            HOSPQTY2 = "0";
            ALLQTY2 = "0";
        }
    }
}