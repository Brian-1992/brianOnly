using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class PH_REPLY : JCLib.Mvc.BaseModel
    {
        public string AGEN_NO { get; set; } //廠商代碼
        public string DNO { get; set; } //交貨批次
        public string MMCODE { get; set; } //院內碼
        public string PO_NO { get; set; } //訂單號碼
        public string SEQ { get; set; } //流水號
        public string BW_SQTY { get; set; } //借貨量
        public string ACEPT_QTY { get; set; } //驗收數量
        public string MEMO { get; set; } //備註
        public string STATUS { get; set; } //狀態
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        // =================================================
        public string PO_TYPE { get; set; }
    }
}