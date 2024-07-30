using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AB0053_3 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼(畫面)
        public string MMNAME_C { get; set; } //中文品名(畫面)
        public string MMNAME_E { get; set; } //英文品名(畫面)
        public string EXP_DATE { get; set; } //月份(畫面)
        public string WARNYM { get; set; } //警示效期(年/月)(畫面)
        public string LOT_NO { get; set; } //藥品批號(畫面)
        public string EXP_QTY { get; set; } //數量(畫面)
        public string MEMO { get; set; } //備註(畫面)
        public string CLOSEFLAG { get; set; } //結案否
        public string CREATE_TIME { get; set; } //建立日期
        public string CREATE_USER { get; set; } //建立人員
        public string UPDATE_IP { get; set; } //異動IP
        public string UPDATE_TIME { get; set; } //異動日期
        public string UPDATE_USER { get; set; } //異動人員
        public string MMCODE_DISPLAY { get; set; } //院內碼(白底顯示)
        public string EXP_DATE_DISPLAY { get; set; } //月份(白底顯示)
        public string LOT_NO_DISPLAY { get; set; } //藥品批號(白底顯示)
        public string WARNYM_TEXT { get; set; } //警示效期(年/月)(紅底顯示)
        public string CLOSEFLAG_NAME { get; set; } //結案否(畫面)
        public string CLOSEFLAG_TEXT { get; set; } //結案否(紅底顯示)
        public string AGEN_NAMEC { get; set; } //廠商
        public string comb_AGEN { get; set; }
        public string MAIL_STATUS { get; set; } //MAIL狀態

        public string MAIL_STATUS_CODE { get; set; }    //mail狀態 原始資料
        public string EMAIL { get; set; }   // 廠商EMAIL
        public string IS_AGENNO { get; set; }   // 廠商是否變更 N-未變更 H-HIS過去資料 P-廠商檔所選資料

        public string WH_EXPQTY { get; set; }

        public string WARNYM_KEY { get; set; }
        public string AGEN_NO { get; set; }
    }
}
