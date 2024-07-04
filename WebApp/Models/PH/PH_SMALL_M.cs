using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class PH_SMALL_M : JCLib.Mvc.BaseModel
    {
        public string DN { get; set; }
        public string ACCEPT { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string AGEN_NO { get; set; }
        public string ALT { get; set; }
        public string APP_INID { get; set; }
        public string APP_USER { get; set; }
        public string APP_USER1 { get; set; }
        public DateTime APPTIME { get; set; }
        public DateTime APPTIME1 { get; set; }
        public string DELIVERY { get; set; }
        public string DEMAND { get; set; }
        public string DEPT { get; set; }
        public string DO_USER { get; set; }
        public string DOTEL { get; set; }
        public string DUEDATE { get; set; }
        public string OTHERS { get; set; }
        public string PAYWAY { get; set; }
        public string PR_NO { get; set; }
        public string REASON { get; set; }
        public string STATUS { get; set; }
        public string TEL { get; set; }
        public string USEWHEN { get; set; }
        public string USEWHERE { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string SIGNDATA { get; set; }
        public string NRSFLAG { get; set; }
        public string NEXT_USER { get; set; }

        
        public string MEMO { get; set; }    //緊急醫療出貨顯示用
        public string IS_CR { get; set; }   //判斷緊急醫療出貨
    }
}