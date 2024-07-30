using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class TC_PURUNCOV : JCLib.Mvc.BaseModel
    {
        public string PUR_UNIT { get; set; }
        public string BASE_UNIT { get; set; }
        public int PURUN_MULTI { get; set; }
        public int BASEUN_MULTI { get; set; }
        public DateTime CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_IP { get; set; }
        public string UPDATE_USER { get; set; }

        // =================== 重凱 ======================
        public string MMCODE { get; set; }
        public string DATA_YM { get; set; }
        public string MMNAME_C { get; set; }    
        public string AGEN_NAMEC { get; set; } 
        public string PUR_QTY { get; set; } 
        public string IN_PURPRICE { get; set; } 
        public string PUR_AMOUNT { get; set; } 
        public string PUR_NOTE { get; set; } 
        public string PUR_DATE { get; set; } 
  



    }
}