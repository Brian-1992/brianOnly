using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_PCAD : JCLib.Mvc.BaseModel
    {
        public string PCACODE { get; set; } //PCA固定處方頭
        public string MMCODE { get; set; } //院內碼(畫面)
        public string MMCODE_TEXT { get; set; } //院內碼(紅底顯示)
        public string MMCODE_DISPLAY { get; set; } //院內碼(白底顯示)
        public string MMNAME_E { get; set; } //英文品名(畫面)
        public string DOSE { get; set; } //劑量(畫面)
        public string CONSUMEFLAG { get; set; } //扣庫
        public string CONSUMEFLAG_DISPLAY { get; set; } //扣庫(畫面顯示)
        public string CONSUMEFLAG_TEXT { get; set; } //扣庫(白框顯示)
        public string COMPUTECODE { get; set; } //計費規則
        public string COMPUTECODE_DISPLAY { get; set; } //計費規則(畫面)
        public string COMPUTECODE_TEXT { get; set; } //計費規則(白框顯示)
        public string E_ORDERUNIT { get; set; } //醫囑單位(畫面)
        public string CREATE_TIME { get; set; }//建立日期
        public string CREATE_ID { get; set; }//建立人員
        public string UPDATE_TIME { get; set; }//異動日期
        public string UPDATE_ID { get; set; }//異動人員
        public string UPDATE_IP { get; set; }//異動IP
    }
}
