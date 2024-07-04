using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class MM_PACK_M : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string DOCTYPE { get; set; }
        public string FLOWID { get; set; }
        public string APPID { get; set; }
        public string APPDEPT { get; set; }
        public string MAT_CLASS { get; set; }
        public string APPLY_NOTE { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string FLOWID_N { get; set; }
        public string MAT_CLASS_N { get; set; }
        public string APP_NAME { get; set; }      // 自訂 APPID || ' ' || USER_NAME(APPID)
        public string APPDEPT_NAME { get; set; }  // 自訂 APPDEPT || ' ' || INID_NAME(APPDEPT)
        public string DOCTYPE_N { get; set; }  

    }
}
