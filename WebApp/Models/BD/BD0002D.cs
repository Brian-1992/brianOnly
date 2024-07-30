using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BD0002D : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; } //院內碼
        public string MMNAME_C { get; set; } //中文品名
        public string MMNAME_E { get; set; } //英文品名
        public string M_PURUN { get; set; } //申購計量單位
        public string PR_QTY { get; set; } //申購數量
        public string PR_PRICE { get; set; } //申購單價
        public string M_CONTPRICE { get; set; } //合約單價
        public string UNIT_SWAP { get; set; } //最小單位
        public string REQ_QTY_T { get; set; } //包裝單位數量
        public string AGEN_NO { get; set; } //廠商代碼
        public string DISC { get; set; } //折讓比
        public string REC_STATUS { get; set; } //申請單狀態
        public string AGEN_NAME { get; set; } //廠商名稱
        public string M_AGENLAB { get; set; } //廠牌
        public string AGEN_TEL { get; set; } //廠商電話
        public string BASE_UNIT { get; set; } //最小單位
    }
}
