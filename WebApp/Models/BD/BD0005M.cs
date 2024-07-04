using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BD0005M : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; } // 01.訂單編號
        public string AGEN_NO { get; set; } // 02.廠商
        public string PO_STATUS { get; set; } // 03.訂單狀態
        public string MEMO { get; set; } // 04.Mail備註
        public string SMEMO { get; set; } // 05.Mail特殊備註

        public string UPDATE_USER { get; set; } // 
        public string UPDATE_IP { get; set; } // 
        // -- 報表用 -- 
        //public string AGEN_NO { get; set; } //
        public string 廠商名稱 { get; set; } //

        public string 院內碼 { get; set; } // 00.院內碼 
        public string 中文品名 { get; set; } // 00.中文品名 
        public string 英文品名 { get; set; } // 00.英文品名 
        public string 廠牌 { get; set; } // 00.廠牌 
        public string 單位 { get; set; } // 00.單位 
        public string 單價 { get; set; } // 00.單價
        public string 數量 { get; set; } // 00.數量
        public string 金額 { get; set; } // 00.金額
        public string 折讓百分比 { get; set; } // 00.折讓百分比


        //public string 院內碼 { get; set; } // 00.院內碼
        public string 單位名稱 { get; set; } // 00.單位名稱
        public string 申請量 { get; set; } // 00.申請量   
        public string M_CONTID { get; set; } // 00.申請量   
        public string TWN_DATE_START { get; set; } 
        public string TWN_DATE_END { get; set; }
        public string MAT_CLASS { get; set; }
    }
}

