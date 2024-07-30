using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CallHisApiResult
    {
        // 執行成功與否
        public bool Success { get; set; }
        // 回傳訊息
        public string Message { get; set; }

        // 資料時間
        public DateTime DataTime { get; set; }
        public IEnumerable<APIResultData> APIResultData { get; set; }

        public CallHisApiResult() { }
    }

    public class APIResultData
    {
        // for AA0012a
        // 
        public string STARTDATATIME { get; set; }
        // 
        public string USEDATETIME { get; set; }
        // 
        public string ORDERCODE { get; set; }
        // 
        public string ORDERENGNAME { get; set; }
        public string SPECNUNIT { get; set; }
        public string FLOORQTY { get; set; }
        public string STKMDRPLIST { get; set; }
        public string NRCODE { get; set; }
        public string STOCKCODE { get; set; }
        public string BEDNO { get; set; }
        public string MEDNO { get; set; }
        public string CHARTNO { get; set; }
        public string CHINNAME { get; set; }
        public string DOSE { get; set; }
        public string ORDERDR { get; set; }
        public string SIGNOPID { get; set; }
        public string PROVEID2 { get; set; }
        public string INOUTQTY { get; set; }
        public string USEQTY { get; set; }
        public string STOCKQTY { get; set; }
        public string RESTQTY { get; set; }
        public string MEMO { get; set; }
        public string INV_QTY { get; set; } // 領用數量
        public string AF_INVQTY { get; set; } // 結存量

        public string CREATEDATETIME { get; set; }  //2020-09-24新增

        public string TYPE { get; set; }

        public string BASE_UNIT { get; set; }

        public APIResultData()
        {
            CREATEDATETIME = string.Empty;
        }
        
    }
}