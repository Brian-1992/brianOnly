using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class PH_MAILLOG : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; } // 01.流水號
        public string LOG_TIME { get; set; } // 02.紀錄時間
        public string MAILFROM { get; set; } // 03.寄件者
        public string MAILTO { get; set; } // 04.收件者
        public string MAILCC { get; set; } // 05.副本收件者
        public string MSUBJECT { get; set; } // 06.主旨
        public string MAILTYPE { get; set; } // 07.信件類別
        public string MAILBODY { get; set; } // 08.MAIL內容
        public string CREATE_USER { get; set; } // 09.建立人員
        public string UPDATE_IP { get; set; } // 10.異動IP


    }
}