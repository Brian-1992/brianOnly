using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class PH_MAILSP_M : JCLib.Mvc.BaseModel
    {
        public string INID { get; set; }
        public string MSGRECNO { get; set; }
        public string MSGNO { get; set; }
        public string MSGTEXT { get; set; }
        public string MEMO { get; set; }
        public string TP { get; set; }
        public string M_CONTID { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string DLINE_DT { get; set; }
        public string SMEMO { get; set; }
        public string REDDISP { get; set; }
        public string CONTRACNO { get; set; }        
    }
}