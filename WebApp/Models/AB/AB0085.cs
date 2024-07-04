using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AB0085 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼(畫面)
        public string MMNAME_C { get; set; } //中文品名(畫面)
        public string MMNAME_E { get; set; } //英文品名(畫面)
        public string LOT_NO { get; set; } //藥品批號(畫面)
        public string REPLY_DATE { get; set; } //回覆效期(畫面)
        public string EXP_QTY { get; set; } //效期藥量(畫面)
        public string EXP_SEQ { get; set; } //效期流水號(畫面)
        public string EXP_DATE1 { get; set; } //最近效期一(畫面)
        public string LOT_NO1 { get; set; } //藥品批號1(畫面)
        public string EXP_DATE2 { get; set; } //最近效期二(畫面)
        public string LOT_NO2 { get; set; } //藥品批號2(畫面)
        public string EXP_DATE3 { get; set; } //最近效期三(畫面)
        public string LOT_NO3 { get; set; } //藥品批號3(畫面)
        public string MEMO { get; set; } //備註(畫面)
        public string REPLY_TIME { get; set; } //回覆日期(畫面)
        public string REPLY_ID { get; set; } //回覆人員(畫面)
        public string CLOSE_TIME { get; set; } //結案日期(畫面)
        public string CLOSE_ID { get; set; } //截止人員(畫面)
        public string EXP_DATE { get; set; } //有效日期(畫面)
        public string WH_NO { get; set; } //庫別代碼(畫面)
        public string WH_NAME { get; set; } //庫別名稱(畫面)
        public string EXP_STAT { get; set; } //效期回覆狀態(畫面)
        public string IP { get; set; } //異動IP
        public string UPDATE_ID { get; set; } //異動ID(畫面)
        public string UPDATE_TIME { get; set; } //異動日期(畫面)
    }
}
