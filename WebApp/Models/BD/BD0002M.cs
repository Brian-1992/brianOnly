using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BD0002M : JCLib.Mvc.BaseModel
    {
        public string PR_NO { get; set; } //申購單號
        public string PR_TIME { get; set; } //申購時間
        public string MAT_CLASS { get; set; } //物料類別
        public string M_STOREID { get; set; } //庫備識別碼
        public string M_STOREID_NAME { get; set; } //庫備識別碼名稱
        public string M_STOREID_CODE { get; set; } //庫備識別碼+名稱
        public string PR_STATUS { get; set; } //申購單狀態代碼
        public string PR_STATUS_NAME { get; set; } //申購單狀態名稱
        public string PR_STATUS_CODE { get; set; } //申購單狀態代碼+名稱
        public string XACTION { get; set; } //申購類別代碼
        public string XACTION_NAME { get; set; } //申購類別名稱
        public string XACTION_CODE { get; set; } //申購類別代碼+名稱

        public string IS_CR { get; set; }
    }
}
