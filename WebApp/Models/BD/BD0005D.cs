using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class BD0005D : JCLib.Mvc.BaseModel
    {
        //public string USERID { get; set; }
        //public string USERNAME { get; set; }
        //public string INID { get; set; }
        //public string INIDNAME { get; set; }
        //public string CENTER_WHNO { get; set; }
        //public string CENTER_WHNAME { get; set; }
        //public string TODAY { get; set; }
        //public string UPDATE_IP { get; set; }
        //public string INID_WHNO { get; set; }
        //public string INID_WHNAME { get; set; }
        //public string TASK_ID { get; set; }

        public string MMCODE { get; set; } // 01.院內碼
        public string MMNAME_C { get; set; } // 02.中文品名
        public string MMNAME_E { get; set; } // 03.英文品名
        public string M_AGENLAB { get; set; } // 04.廠牌
        public string M_PURUN { get; set; } // 05.單位
        public string PO_PRICE { get; set; } // 06.單價
        public string PO_QTY { get; set; } // 07.申購量
        public string PO_AMT { get; set; } // 08.單筆價
        public string MEMO { get; set; } // 09.備註
        public string PO_NO { get; set; } // 10.


    }
}
