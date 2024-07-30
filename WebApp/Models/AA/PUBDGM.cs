using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class PUBDGM : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string APPDEPT { get; set; }
        public string APPTIME { get; set; }
        public string APPTIME_T { get; set; }
        public string APPID { get; set; }
        public string MEMO { get; set; }
        public string STATUS { get; set; }
        public string APVTIME { get; set; }
        public string IS_DEL { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string APP_AMOUNT { get; set; }
    }
}
