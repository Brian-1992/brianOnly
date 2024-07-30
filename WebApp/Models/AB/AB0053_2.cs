using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class AB0053_2 : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼(畫面)
        public string MMNAME_C { get; set; } //中文品名(畫面)
        public string MMNAME_E { get; set; } //英文品名(畫面)
        public string EXP_DATE { get; set; } //最近效期(畫面)
        public string LOT_NO { get; set; } //藥品批號(畫面)
        public string REPLY_DATE { get; set; } //回覆效期(畫面)
        public string PH1S { get; set; } //藥庫(畫面)
        public string CHEMO { get; set; } //內湖化療調配室(畫面)
        public string CHEMOT { get; set; } //汀洲化療調配室(畫面)
        public string PH1A { get; set; } //內湖住院藥局(畫面)
        public string PH1C { get; set; } //內湖門診藥局(畫面)
        public string PH1R { get; set; } //內湖急診藥局(畫面)
        public string PHMC { get; set; } //汀洲藥局(畫面)
        public string TPN { get; set; } //製劑室(畫面)
        public string BASE_UNIT { get; set; } //劑量單位(畫面)
        public string E_MANUFACT { get; set; } //廠商(畫面)
        public string EXP_QTY { get; set; } //總數(畫面)
    }
}
