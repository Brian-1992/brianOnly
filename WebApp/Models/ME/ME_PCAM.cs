using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class ME_PCAM : JCLib.Mvc.BaseModel
    {
        public string PCACODE { get; set; } //PCA固定處方頭(畫面)
        public string PCACODE_TEXT { get; set; } //PCA固定處方頭(紅底顯示)
        public string PCACODE_DISPLAY { get; set; } //PCA固定處方頭(白底顯示)
        public string MMNAME_E { get; set; } //英文品茗(畫面)
        public string DOSE { get; set; } //劑量(畫面)
        public string FREQNO { get; set; } //院內頻率(畫面)
        public string E_PATHNO { get; set; } //使用途徑(畫面)
        public string E_ORDERUNIT { get; set; } //醫囑單位(畫面)
        public string CREATE_TIME { get; set; }//建立日期
        public string CREATE_ID { get; set; }//建立人員
        public string UPDATE_TIME { get; set; }//異動日期
        public string UPDATE_ID { get; set; }//異動人員
        public string UPDATE_IP { get; set; }//異動IP
    }
}
