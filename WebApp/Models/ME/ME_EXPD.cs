using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ME_EXPD : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string EXP_DATE { get; set; }
        public string EXP_SEQ { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE1 { get; set; }
        public string LOT_NO1 { get; set; }
        public string EXP_DATE2 { get; set; }
        public string LOT_NO2 { get; set; }
        public string EXP_DATE3 { get; set; }
        public string LOT_NO3 { get; set; }
        public string EXP_STAT { get; set; }
        public string EXP_QTY { get; set; }
        public string REPLY_DATE { get; set; }
        public string MEMO { get; set; }
        public string REPLY_TIME { get; set; }
        public string REPLY_ID { get; set; }
        public string CLOSE_TIME { get; set; }
        public string CLOSE_ID { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_ID { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_ID { get; set; }
        public string IP { get; set; }

        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string WH_NAME { get; set; }
        public string EXP_STAT_NAME { get; set; }
        public string MMCODE_DISPLAY { get; set; }
        public string LOT_NO_DISPLAY { get; set; }
        public string REPLY_DATE_DISPLAY { get; set; }

        public string STORE_LOC { get; set; }

        public string INV_QTY { get; set; }

        // 匯入用
        public string UPLOAD_FAIL_REASON { get; set; }
        public string UPLOAD_ROW_NUMBER { get; set; }
    }
}