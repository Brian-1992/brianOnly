using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class TC_INVQMTR : JCLib.Mvc.BaseModel
    {
        public string DATA_YM { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string BASE_UNIT { get; set; }
        public string IN_PRICE { get; set; }
        public string PMN_INVQTY { get; set; }
        public string MN_INQTY { get; set; }
        public string MN_USEQTY { get; set; }
        public string MN_INVQTY { get; set; }
        public string STORE_LOC { get; set; }
        public string M6AVG_USEQTY { get; set; }
        public string M3AVG_USEQTY { get; set; }
        public string M6MAX_USEQTY { get; set; }
        public string M3MAX_USEQTY { get; set; }
        public string INV_DAY { get; set; }
        public string EXP_PURQTY { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string PUR_UNIT { get; set; }
        public string IN_PURPRICE { get; set; }
        public string BASEUN_MULTI { get; set; }
        public string PURUN_MULTI { get; set; }
        public string RCM_PURQTY { get; set; }
        public string PUR_QTY { get; set; }
        public string PURCH_ST { get; set; }
        public string PUR_NOTE { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string RCM_PURDAY { get; set; }


        public string PURCH_ST_NAME { get; set; }
        public string ITEM_STRING { get; set; }


        public string EXP_PURDAY { get; set; }  // 應訂購天數
        public string AGEN_COUNT { get; set; }  // 廠商數目
        public string PUR_SEQ { get; set; }     // 採購順序
        public string IS_VALID { get; set; }    // 是否可訂購
        public string PUR_DAY { get; set; }     // 訂購天數


        public IEnumerable<AGEN_INFO> AGENS { get; set; }
    }

    public class AGEN_INFO : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string AGEN_NAMEC { get; set; }  
        public string PUR_UNIT { get; set; }        // 單位劑量
        public string IN_PURPRICE { get; set; }     // 進貨單價
        public string BASE_UNIT { get; set; }       // 計量單位
        public string IS_VALID { get; set; }
        public string PURUN_MULTI { get; set; }     // 單位劑量乘數
        public string BASEUN_MULTI { get; set; }    // 計量單位乘數
        public string PUR_QTY { get; set; }         // 訂購量
        public string PUR_DAY { get; set; }         // 訂購天數

        public string RCM_PURQTY { get; set; }      // 建議訂購量
        public string RCM_PURDAY { get; set; }      // 建議訂購天數

    }
}